import { basename } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { Output, setting } from '@rasm/mcp/cli';
import { Handle, Server } from '@rasm/mcp/server';
import { ToolCall } from '@rasm/mcp/toolkit';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, ByteSize, Cause, Context, DateTime, Duration, Effect, Equal, Function, HashMap, HashSet, Match, Option, Order, Predicate, pipe, Record, Ref, Request, RequestResolver, Result, Schema, Stream, String, Struct, SubscriptionRef, Tuple, UndefinedOr } from 'effect';
import { type McpServer, McpSchema, Tool } from 'effect/ai';
import { Flag } from 'effect/cli';
import type { RpcClient, RpcClientError, RpcGroup } from 'effect/rpc';
import { Address, type Index, type Item, type Shown } from '#/address';
import { type Capture, Recording } from '#/capture';
import { ApplicationServer } from '#/client';
import { ApplicationFailure } from '#/contract';
import { AppleEventValue, type Installation, type ScriptingDictionary } from '#/installations';
import { Event, InstanceSnapshot, InstanceState, type Pasteboard, QuitMode, type Running, type Session, Written } from '#/instances';
import { Container, Cursor, type Frame, Kind, type Model, Scope, Value } from '#/interface';
import { type InterfaceMap, InterfaceMaps } from '#/interface-map';
import { Outline } from '#/outline';
import { Diagnostics, type Pressure } from '#/processes';
import { type Declaration, Description, type SourceRpcs } from '#/source';
import { Act } from '#/steps';
import { describeUsage, type Entry, follow, Proving, Workspace } from '#/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

type Scripting<E, R> = (target: Effect.Success<Workspace['Service']['current']>, code: string) => Effect.Effect<Tool.Success<typeof TOOLS.execute_script>, E, R>;
type Linked = RpcClient.RpcClient.Flat<RpcGroup.Rpcs<typeof SourceRpcs>, RpcClientError.RpcClientError>;
type Drawn = Option.Option<{ readonly instance: InstanceSnapshot['instance']; readonly marks: ReturnType<typeof marksOf> }>;
type Step = Tool.Parameters<typeof TOOLS.act>['steps'][number];
type Sample = Exclude<Extract<Step, { readonly pointer: unknown }>['pointer'], string>;
type Route = (ref: number) => Option.Option<Description['containers'][number]['locator']>;
type Acted = typeof Act.Result.Type;
type Delivery = Result.Result.Success<Acted['outcomes'][number]>;
type Rows = Running['windows'];
interface Run<A> {
    readonly entries: readonly A[];
    readonly outcomes: Acted['outcomes'];
    readonly acted: Option.Option<Acted>;
}
interface Phrase {
    readonly verb: keyof typeof VERBS;
    readonly object: string;
}
interface Explains {
    readonly pointed?: readonly string[];
    readonly help?: string | undefined;
    readonly troubled?: boolean;
    readonly keyed?: boolean;
}
interface Logged {
    readonly file: Option.Option<string>;
    readonly timestamp: Option.Option<string>;
    readonly pieces: Array.NonEmptyReadonlyArray<string>;
    readonly key: string;
}
interface Placement {
    readonly instance: InstanceSnapshot['instance'];
    readonly windows: readonly number[];
    readonly frame: typeof Frame.Type;
    readonly size: readonly [number, number];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const LIMIT = 50;
const FRAMES = 6;
const PROOFS = {
    answer: 'the application accepted it',
    change: Outline.say.changed,
    value: 'the value reads back',
    limit: 'at end of range',
    posting: 'posted',
    unanswered: 'no answer in time, so it may have run',
    exit: 'the application exited',
} as const satisfies Record<Delivery['proof'], string>;
const VERBS = {
    click: ['Clicked', 'Clicking', 'click'],
    rightClick: ['Right-clicked', 'Right-clicking', 'right-click'],
    middleClick: ['Middle-clicked', 'Middle-clicking', 'middle-click'],
    doubleClick: ['Double-clicked', 'Double-clicking', 'double-click'],
    hover: ['Hovered over', 'Hovering over', 'hover over'],
    scroll: ['Scrolled', 'Scrolling', 'scroll'],
    drag: ['Dragged', 'Dragging', 'drag'],
    press: ['Pressed', 'Pressing', 'press'],
    run: ['Ran', 'Running', 'run'],
    set: ['Set', 'Setting', 'set'],
    select: ['Selected text in', 'Selecting text in', 'select text in'],
} as const satisfies Record<string, readonly [done: string, doing: string, todo: string]>;
const POLICIES: Record.ReadonlyRecord<string, string> = { accessory: 'menu-bar application', prohibited: 'background application' };
const NUMBER = /(?<number>\p{Nd}+(?:\.\p{Nd}+)?)/u;
const CHOICES = new Intl.ListFormat('en', { type: 'disjunction' });
const INSTRUCTIONS = [
    'Start with applications: it lists what is installed and running, and open makes one application the target of later calls. Each result says what changed since your last call and, where an answer or a continuation is due, the call that gives it.',
    'Every container read in an application joins its interface map with its controls, kept per build. Search it with inspect find before opening anything, as it holds closed menus and dialogs. Act on a remembered item by the path find prints: the call presses a menu item with its menu closed and opens what leads to a dialog, starts at the route that last passed, and reads each step back, so read again only what a result names as changed or refused.',
    'To map an application, applications for that one application counts the menus, dialogs, and panels its interface map holds and the openers it never saw open, which inspect unopened lists.',
] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Side = setting('longest-side', Schema.FiniteFromString.pipe(Schema.decodeTo(Schema.Int.check(Schema.isGreaterThan(0)))), Flag.withDefault(1568), 'Longest side in pixels of each image a view returns');
const Reply = Schema.toCodecJson(AppleEventValue);
const Pixel = Schema.Finite.check(Schema.isGreaterThanOrEqualTo(0));
const Pixels = Schema.Tuple([Pixel, Pixel, Schema.Finite.check(Schema.isGreaterThan(0)), Schema.Finite.check(Schema.isGreaterThan(0))]);
const LogRecord = Schema.fromJsonString(
    Schema.Struct({ timestamp: Schema.String, severity_text: Schema.String, scope: Schema.optionalKey(Schema.String), body: Schema.String, attributes: Schema.optionalKey(Schema.Record(Schema.String, Schema.Json)) }).annotate({ description: 'OpenTelemetry log record a server process writes as one JSON line' }),
);
const TOOLS = {
    applications: Tool.make('applications', {
        description:
            'Lists installed applications one line each, with the state, windows, and status items of a running instance and the MCP server driving one another server claims, or one application in full: its installation, App Intents actions, open command parameters, interface map per language, and each running instance with its footprint and uptime. open launches an application behind, or reopens its running instance behind as the Dock does, makes it the target of later calls, and prints what it shows as inspect does; close quits it, or every application this handle opened, a line per instance saying whether it quit',
        parameters: Schema.Struct({
            action: Schema.optionalKey(Schema.Literals(['list', 'open', 'close']).annotate({ description: 'list when absent' })),
            application: Schema.optionalKey(
                Schema.String.annotate({
                    description: 'Listed name, product, bundle identifier, alias, or bundle URL, a product listed in several forms taking the first form listed; open without it takes the application the system opens the documents with, close without it quits every application this handle opened',
                }),
            ),
            documents: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: "File paths or URLs the application opens; list without application lists every application opening them, the system's default first" })),
            arguments: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Process arguments of a new instance, refused where a running instance would receive them' })),
            environment: Schema.optionalKey(Schema.Record(Schema.String, Schema.String).annotate({ description: 'Environment variables of a new instance, refused as arguments are' })),
            new: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Open an instance beside the running ones' })),
            activate: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Bring the target in front and leave it there, where a container asks for something only the person can give' })),
            opening: Schema.optionalKey(Schema.Record(Schema.String, Schema.Union([Schema.Boolean, Schema.Finite, Schema.String])).annotate({ description: 'Open command parameters by the names the listing of the application states, sent with the documents once the instance is idle' })),
            mode: Schema.optionalKey(QuitMode),
            wait: Schema.optionalKey(Schema.Finite.check(Schema.isGreaterThan(0)).annotate({ description: 'Seconds open follows the instance, returning its last state, or close waits at most' })),
        }),
        success: Schema.String,
    }).annotateMerge(Context.make(Tool.Destructive, false).pipe(Context.add(Tool.Idempotent, true), Context.add(Tool.OpenWorld, false), Context.add(Tool.Meta, { 'anthropic/alwaysLoad': true }))),
    view: Tool.make('view', {
        description:
            'Shows windows of the target as images, each led by a line naming the windows it composites by reference and title: the front window, every window with all (hidden, minimized, and off-Space ones from their last drawn content), the windows showing a document, an object a linked source renders, a reference cropped from its window, or a pixel rectangle of the last image at full resolution. A menu, or the item opening it, shows each open level with its outline, and a remembered container its outline from the map, nothing opened. With seconds it records and returns key frames: the first and last state whole, each state between cropped to its change, each line naming when it appeared and what changed in pixels of the first image. Points act takes count pixels of the last image a view returned',
        parameters: Schema.Struct({
            of: Schema.optionalKey(
                Schema.Union([Schema.Literals(['front', 'all', 'document', 'object']), Address.At, Pixels.annotate({ description: 'Pixel rectangle [x, y, width, height] of the last image a view returned, shown again at the full resolution of the screen' })]).annotate({
                    description: 'front when absent; all for every window of the target; document or object with id; else a reference or label path as inspect prints it, or a pixel rectangle',
                }),
            ),
            id: Schema.optionalKey(Schema.String.annotate({ description: 'document: document id from a result; object: id a script result gives the object' })),
            marks: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Draw each control of the target in the image with its frame and reference, as act takes it, each label kept inside the image beside a frame at its edge' })),
            seconds: Schema.optionalKey(Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: 60, exclusiveMinimum: true })).annotate({ description: 'Seconds to record what the view shows, the windows the application opens meanwhile included' })),
            frames: Schema.Union([Schema.Literal(1).annotate({ description: 'One still image, as view without seconds returns' }), Recording.fields.frames])
                .annotate({ description: 'Most key frames a recording with seconds returns, from 2; 1 returns one still image in place of a recording', default: FRAMES })
                .pipe(Schema.withDecodingDefaultKey(Effect.succeed(FRAMES))),
        }),
        success: Schema.String,
    }).annotate(Tool.Readonly, true),
    inspect: Tool.make('inspect', {
        description:
            'Reads the interface of the target as an outline: the application name, with any departure from responding and behind, then each container and its controls, each led by a reference such as @a12 with its label, kind, value, and what it takes, and what was shown before and is gone marked not shown now. A menu item names the menu it opens, its shortcut as the act key step pressing it, and the item a modifier puts in its place, and the first line names the current tool of the pointer. A cut ends with the call that continues it, and a dialog asking for an answer with the act call that gives it. find searches every container shown and remembered, a remembered hit naming the path act takes. Only in clipboard prints the clipboard, its reference taking text through act to',
        parameters: Schema.Struct({
            in: Schema.optionalKey(
                Schema.Union([Schema.Literal('clipboard').annotate({ description: 'The clipboard read alone, its text included' }), Address.At]).annotate({
                    description: 'Container, choice, multi, text, menu item, or remembered container read alone, a menu item reading its menu, a multi its rows, and a text control its style runs with their fonts, colors, links, and frames; clipboard reads the clipboard; every container when absent',
                }),
            ),
            find: Schema.optionalKey(Schema.String.annotate({ description: 'Text searched in labels, values, and help of every container shown and remembered, best match and live hits first, a remembered hit printing the path act takes and the item that opens it' })),
            kind: Schema.optionalKey(Kind.annotate({ description: 'Kind of control find keeps, or every control of the kind without find' })),
            unopened: Schema.optionalKey(Schema.Boolean.annotate({ description: 'List the items the map saw open nothing it has read: an item whose menu or popover was never read, an item whose label ends in an ellipsis with no dialog seen, and a choice whose options never listed' })),
            after: Schema.optionalKey(Address.At.annotate({ description: 'Item after which the page starts, as a cut names it' })),
            wait: Schema.optionalKey(Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: 60, exclusiveMinimum: true })).annotate({ description: 'Seconds to wait for the next change of the target before reading, marking what changed' })),
        }),
        success: Schema.String,
    }).annotate(Tool.Readonly, true),
    act: Tool.make('act', {
        description:
            'Runs steps on the target in order without bringing it forward, each aimed at a reference or label path as inspect prints it: at presses it, with to sets the value its kind takes (an option label chooses, text on the clipboard reference copies), with range selects or replaces text; key posts a key with modifiers; pointer clicks, drags, strokes, hovers, or scrolls at a reference or a point [x, y] in pixels of the last image a view returned. A remembered item runs in the same call: a menu item pressed with its menu closed, an item of a dialog after the items that open it. A pointer step on a canvas a linked source states draws through the host. Each step starts at an Accessibility call, or the route that last passed on the control, moves through background event routes, and reads back what it changed, the system route last bringing the target forward. A malformed step refuses the call before any step runs. Returns the application name with any departure from normal, a line per step saying what was done and what showed it, a failure ending the run with its reason and the steps not run, then the containers the steps opened and the controls whose values changed as inspect prints them, the documents written, and the clipboard where a step wrote it or the target copied the text it shows selected; inspect in clipboard reads any other copy. With record it returns key frames of the steps as view with seconds does',
        parameters: Schema.Struct({
            steps: Act.steps(Address.At, {}, {}),
            record: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Record the front window, else the control the first step aims at, while the steps run, returning the state before them and the last state whole, each state between cropped to its change; steps a linked source runs go unrecorded' })),
        }),
        success: Schema.String,
    }).annotateMerge(Context.make(Tool.Destructive, false).pipe(Context.add(Tool.OpenWorld, false))),
    execute_script: Tool.make('execute_script', {
        description: 'Runs code through the script command the scripting dictionary of the target declares and returns its reply as JSON, refused with the commands of the dictionary where it declares none; a dialog, an exit, or a hang returns the call at once, its result arriving under finished in a later result',
        parameters: Schema.Struct({ code: Schema.String.annotate({ description: 'Source the script runtime of the target runs' }) }),
        success: Schema.Struct({ result: Schema.OptionFromOptionalKey(Schema.Json), written: Schema.optionalKey(Written) }),
    }).annotate(Tool.OpenWorld, false),
    diagnostics: Tool.make('diagnostics', {
        description:
            'Reads diagnostics newest first, one line each: without application, the log lines of the server under the file holding them; with one, its crash, hang, and diagnostic reports, the sample report taken each time it stopped responding, and the errors, faults, and lines of its own in its unified log. A run of lines alike but for their numbers prints once with its count, its first and last time, and the range of each number that varied',
        parameters: Schema.Struct({
            application: Schema.optionalKey(Schema.String.annotate({ description: 'Listed name, product, bundle identifier, alias, or bundle URL, a product listed in several forms taking the first form listed; the server itself when absent' })),
            since: Diagnostics.fields.since,
            limit: Diagnostics.fields.limit.annotate({ default: LIMIT }).pipe(Schema.withDecodingDefaultKey(Effect.succeed(LIMIT))),
            find: Diagnostics.fields.find,
        }),
        success: Schema.String,
    }).annotate(Tool.Readonly, true),
};
class Interned extends Request.TaggedClass('Interned')<{ readonly instance: InstanceSnapshot['instance']; readonly locator: string }, number, Effect.Error<ReturnType<ApplicationServer['Service']['intern']>>> {}

// --- [SERVICES] ------------------------------------------------------------------------

const Viewed = Context.Reference<Option.Option<Placement>>('@rasm/applications/tools/Viewed', { defaultValue: Option.none });
const Indexed = Context.Reference<Option.Option<{ readonly instance: InstanceSnapshot['instance']; readonly index: Index }>>('@rasm/applications/tools/Indexed', { defaultValue: Option.none });
const Clip = Context.Reference<Option.Option<{ readonly seconds: number; readonly frames: number }>>('@rasm/applications/tools/Clip', { defaultValue: Option.none });

// --- [OPERATIONS] ----------------------------------------------------------------------

const screenOf = ({ frame: [[left, top], [width, height]], size: [columns, rows] }: Placement, [x, y, wide, high]: readonly [number, number, number, number]) =>
    x + wide <= columns && y + high <= rows
        ? Result.succeed<typeof Frame.Type>([
              [left + (x * width) / columns, top + (y * height) / rows],
              [(wide * width) / columns, (high * height) / rows],
          ])
        : Result.fail(new Cause.IllegalArgumentError(`The rectangle at ${x},${y}, ${wide} by ${high}, leaves the last image a view returned, which is ${columns} by ${rows} pixels`));
const pointOf = (viewed: Option.Option<Placement>, [x, y, ...rest]: Sample) =>
    Option.match(viewed, {
        onNone: () => Result.fail(new Cause.IllegalArgumentError(`${x},${y} counts pixels of the last image a view of the target returned, and this handle holds none, view the target first`)),
        onSome: (placement) => Result.map(screenOf(placement, [x, y, 0, 0]), ([[left, top]]): Sample => [left, top, ...rest]),
    });
const marksOf = (index: Index) => index.shown.flatMap(({ controls }) => controls.flatMap(({ control, item }) => Option.toArray(Option.all({ frame: Option.fromUndefinedOr(control.frame), label: item.token }))));
const locate = (document: string) => (URL.canParse(document) ? new URL(document) : pathToFileURL(document));
const windowsOf = (state: InstanceSnapshot['state']): Rows => InstanceState.matchOrElse(state, { running: Struct.get('windows') }, Array.empty);
const presentedWindows = (state: InstanceSnapshot['state']) => windowsOf(state).filter((window) => window.presented && window.kind !== 'menu');
const frontOf = (state: InstanceSnapshot['state']) =>
    Option.map(
        Option.orElse(
            Array.findFirst(presentedWindows(state), (window) => window.asking || window.kind === 'window'),
            () => Array.head(presentedWindows(state)),
        ),
        Struct.get('number'),
    );
const presented = (presentation: (typeof Capture.Screenshot.Type)['presentation']) => Option.toArray(Option.filter(Option.fromUndefinedOr(presentation), ({ _tag }) => _tag !== 'behind')).map((held) => (held._tag === 'restored' && held.back ? 'came in front and was put back' : 'came in front and stayed there'));
const unshownOf = (row: Entry, state: InstanceSnapshot['state'], index: Index, { documentTypes }: Installation) => {
    const items = index.shown
        .filter(({ container }) => container.kind === 'statusItem')
        .flatMap(({ controls }) => controls)
        .flatMap(({ item }) => Option.toArray(Option.map(item.token, (token) => ({ token, named: [token, ...Option.toArray(Option.map(item.name, Outline.named))].join(' ') }))));
    const hints = [
        ...(windowsOf(state).some((window) => window.kind !== 'menu') ? ['view {of:"all"} shows its windows from their last drawn content'] : []),
        ...items.map(({ token, named }) => `act {steps:[{at:"${token}"}]} opens the menu or panel of its status item ${named}`),
        ...(Option.exists(documentTypes, Array.isReadonlyArrayNonEmpty) ? [`applications {action:"open", application:${JSON.stringify(row.name)}, documents:["<path>"]} opens a document window`] : []),
    ];
    return Array.match(hints, { onEmpty: () => `${row.name} shows no window and no status item`, onNonEmpty: (listed) => `${row.name} shows no window, ${CHOICES.format(listed)}` });
};
const responding = (state: InstanceSnapshot['state']) => InstanceState.matchOrElse(state, { running: ({ status }) => InstanceState.Status.guards.responding(status) }, Function.constFalse);
const instanceName = (each: InstanceSnapshot) => each.name ?? each.identifier;
const runOf = (held: Index, ref: number) =>
    held.shown.flatMap(({ controls }) =>
        Option.match(
            Array.findFirstIndex(controls, ({ control }) => control.ref === ref),
            { onNone: Array.empty, onSome: (at) => Array.takeWhile(Array.drop(controls, at + 1), ({ control }) => control.kind === 'row' || control.row !== undefined) },
        ),
    );
const rowsOf = (held: Index, ref: number) => runOf(held, ref).filter(({ control }) => control.kind === 'row');
const scrolling = (token: string) => `act {steps:[{pointer:${JSON.stringify(token)}, by:[0,-400], button:"none"}]}`;
const memberOf = (held: Index, scope: Option.Option<Item>): Predicate.Predicate<Item> =>
    Option.match(scope, {
        onNone: () => Function.constTrue,
        onSome: (chosen) =>
            Address.Target.$match(chosen.target, {
                Container:
                    ({ container }) =>
                    (each: Item) =>
                        each.target._tag === 'Control' && each.target.container.ref === container.ref,
                Control: ({ control }) => {
                    const listed = new Set(runOf(held, control.ref).map((each) => each.control.ref));
                    return (each: Item) => each.target._tag === 'Control' && listed.has(each.target.control.ref);
                },
                Option: () => Function.constTrue,
                RememberedContainer:
                    ({ locator }) =>
                    (each: Item) =>
                        each.target._tag === 'RememberedControl' && each.target.container === locator,
                RememberedControl: () => Function.constTrue,
            }),
    });
const partialOf = (held: Index) =>
    held.shown.flatMap(({ controls }) =>
        controls.flatMap(({ control, item }) => {
            const loaded = rowsOf(held, control.ref).length;
            return control.kind === 'multi' && (control.total ?? 0) > loaded ? [{ control, item, loaded }] : [];
        }),
    );
const joinedOf = (held: Index, reads: readonly { readonly containers: Model['containers'] }[]) =>
    held.model.containers.map((container) => ({
        ...container,
        controls: Array.dedupeWith([...container.controls, ...reads.flatMap(({ containers }) => containers.filter((read) => read.ref === container.ref).flatMap((read) => read.controls))], (left, right) => left.ref === right.ref),
    }));
const unreadOf = ({ control, item, loaded }: ReturnType<typeof partialOf>[number], read: Index) => {
    const remaining = (control.total ?? loaded) - loaded - rowsOf(read, control.ref).length;
    return remaining > 0 ? Option.toArray(Option.map(item.token, (token) => `${token}, ${Outline.count(remaining, 'row')} unread - ${scrolling(token)}`)) : [];
};
const neverOpened = (amount: number) => `[UNKNOWN]: ${Outline.count(amount, 'item')} - ${Outline.call([Outline.quoted('unopened', true)])}`;
const describeState = (state: InstanceSnapshot['state']) =>
    InstanceState.match(state, {
        running: (activity) => [...InstanceState.Status.matchOrElse(activity.status, { responding: () => [], unresponsive: ({ thread }) => [`not responding, main thread ${thread}${describeUsage(activity)}`] }, ({ _tag }) => [_tag]), ...(activity.active ? ['in front'] : [])],
        failed: ({ reason, subject, message }) => [`state unread, ${message ?? `${subject} ${reason}`}`],
        exited: () => ['exited'],
    });
const describeRow = (row: Entry) => [
    row.name,
    ...Record.collect(
        Record.filter(row.facts, (value) => value !== row.identifier),
        (key, value) => `${key} ${Predicate.isString(value) ? value : JSON.stringify(value)}`,
    ),
    ...Option.toArray(Option.map(row.claim, ({ server }) => `driven by the ${server} MCP server, use its tools`)),
];
const describeInstance =
    (row: Entry, now: Option.Option<DateTime.Utc>) =>
    ({ url, state, usage, usageFailure, launchDate }: InstanceSnapshot) => [
        ...Array.match(presentedWindows(state), { onEmpty: Array.empty, onNonEmpty: (shown) => [Outline.count(shown.length, 'window')] }),
        ...InstanceState.matchOrElse(state, { running: ({ statusItems = 0, statusItemsFailure }) => [...(statusItems > 0 ? [Outline.count(statusItems, 'statusItem')] : []), ...Array.fromNullishOr(statusItemsFailure).map((message) => `status items unread, ${message}`)] }, Array.empty),
        ...Array.fromNullishOr(usage).flatMap(({ footprint, cpu }) => [`${ByteSize.format(ByteSize.bytes(footprint))} of memory`, `${Outline.duration(Duration.seconds(cpu))} of processor time`]),
        ...Array.fromNullishOr(usageFailure).map((message) => `usage unread, ${message}`),
        ...Option.toArray(Option.flatMap(now, (at) => Option.map(Option.fromUndefinedOr(launchDate), (launched) => `up for ${Outline.duration(DateTime.distance(launched, at))}`))),
        ...(url.href === row.url.href ? [] : [`from ${fileURLToPath(url)}`]),
    ];
const describeInstallation = ({ url, activationPolicy, scriptable, multipleInstancesProhibited, documentTypes, urlSchemes, quarantine, appIntents, unread = [] }: Installation, open: ScriptingDictionary['open']) => {
    const listed = Record.getSomes({
        documents: Option.map(documentTypes, (types) => types.flatMap(({ extensions }) => extensions.map((extension) => `.${extension}`))),
        'URL schemes': urlSchemes,
        'open takes': Option.map(open, ({ parameters }) => parameters.map(({ name }) => name)),
    });
    return {
        head: [`${Option.getOrElse(Record.get(POLICIES, activationPolicy), () => 'an application')} at ${fileURLToPath(url)}`, ...(Option.contains(scriptable, true) ? ['scriptable'] : []), ...(Option.contains(multipleInstancesProhibited, true) ? ['one instance at most'] : [])],
        rest: [
            ...Record.collect(
                Record.filter(listed, (held) => Array.isReadonlyArrayNonEmpty(held)),
                (label, held) => `${label}: ${held.join(', ')}`,
            ),
            ...Option.toArray(quarantine).map(({ agentName, originURL }) => ['quarantined', ...Array.fromNullishOr(agentName).map((agent) => `by ${agent}`), ...Array.fromNullishOr(originURL).map((origin) => `from ${origin}`)].join(' ')),
            ...Option.getOrElse(appIntents, Array.empty).map(({ identifier: intent, title, openAppWhenRun, parameters }) =>
                [`App Intent ${Outline.named(title ?? intent)}`, ...(openAppWhenRun ? ['opens the application'] : []), ...Array.match(parameters, { onEmpty: Array.empty, onNonEmpty: (held) => [`takes ${Outline.list(held.map(({ name, isOptional }) => (isOptional ? `${name} (optional)` : name)))}`] })].join(', '),
            ),
            ...unread.map(({ subject, reason, message }) => [`could not read ${subject}, ${reason}`, ...Array.fromNullishOr(message)].join(': ')),
        ],
    };
};
const describeCoverage = (language: Option.Option<string>, { known, earlier, openers }: InterfaceMap['coverage']) => {
    const kinds = Record.toEntries(known).flatMap(([kind, amount]) => (Schema.is(Container.fields.kind)(kind) ? [Outline.count(amount, kind)] : []));
    const fields = [
        ...Option.match(language, { onNone: Array.empty, onSome: (held) => [`[LANGUAGE]: ${held}`] }),
        `[KNOWN]: ${Array.match(kinds, { onEmpty: () => 'nothing', onNonEmpty: Array.join(', ') })}`,
        ...(earlier > 0 ? [`[EARLIER_BUILDS]: ${earlier}`] : []),
        ...Array.match(openers, { onEmpty: Array.empty, onNonEmpty: (never) => [neverOpened(never.length)] }),
    ];
    return fields.join(' | ');
};
const stateLine = (application: string, state: InstanceSnapshot['state'], { pointed = [], help, screen, strain, troubled = false, keyed = false }: Explains & { readonly screen: Option.Option<Session>; readonly strain: Option.Option<Pressure> }) => {
    const strained = troubled || !responding(state);
    const conditions = [
        ...Option.match(screen, {
            onNone: Array.empty,
            onSome: ({ onConsole, locked, asleep, secureInput }) => Record.keys(Record.filter({ 'another user session holds the screen': !onConsole, 'the screen is locked': locked, 'the display sleeps': asleep, 'secure input is on, so keys do not reach it': keyed && secureInput }, Boolean)),
        }),
        ...Option.match(
            Option.filter(strain, () => strained),
            {
                onNone: Array.empty,
                onSome: ({ memory, thermal, advisory = {} }) => [
                    ...(memory === 'normal' ? [] : [`memory pressure ${memory}`]),
                    ...(thermal === 'serious' || thermal === 'critical' ? [`thermal state ${thermal}`] : []),
                    ...(Record.some(advisory, (level) => level === 'bad') ? ['the system advises against extra work'] : []),
                ],
            },
        ),
    ];
    return [[application, ...describeState(state), ...pointed, ...conditions].join(', '), ...Array.fromNullishOr(help)].join('\n');
};
const showing = (instances: readonly InstanceSnapshot[], id: string) =>
    instances.flatMap((each) =>
        Array.match(
            windowsOf(each.state)
                .filter((window) => window.document === id)
                .map(Struct.get('number')),
            { onEmpty: Array.empty, onNonEmpty: (numbers) => [{ each, numbers }] },
        ),
    );
const shot = (application: Option.Option<string>, instance: InstanceSnapshot['instance'], rows: Rows, lead: readonly string[]) => {
    const names = Record.fromIterableWith(rows, ({ number, container, kind = 'window', title }) => [`${number}`, [...Array.fromNullishOr(container).map((ref) => Address.token(kind, ref)), Outline.noun(kind), ...Array.fromNullishOr(title).map(Outline.named)].join(' ')]);
    return ({ png, width, height, frame, windows, backingStore, document, presentation }: typeof Capture.Screenshot.Type) => ({
        line: [
            ...Option.toArray(application),
            ...lead,
            ...Array.match(windows, {
                onEmpty: () => ['drawn from what was read, as the application shows no window for it'],
                onNonEmpty: (numbers) => numbers.map((number) => Option.getOrElse(Record.get(names, `${number}`), () => 'a window not read yet')),
            }),
            ...(backingStore ? ['off screen, its last drawn content'] : []),
            ...presented(presentation),
            ...Array.fromNullishOr(document).map((url) => `document: ${url}`),
        ].join(', '),
        image: Option.some({ png, placement: Option.some<Placement>({ instance, windows, frame, size: [width, height] }) }),
    });
};
const keyFramesOf = (application: Option.Option<string>, instance: InstanceSnapshot['instance'], rows: Rows, recording: typeof Recording.Result.Type) => {
    const placed = Function.memoize((lead: readonly string[]) => shot(application, instance, rows, lead));
    return [
        ...Array.zipWith(recording.keyFrames, Recording.leads(recording), ({ screenshot }, lead) => placed(lead)(screenshot)),
        ...(recording.keyFrames.some(({ changed }) => changed !== undefined) ? [{ line: 'Changed rectangles count pixels of the first image, view {of:[x, y, width, height]} shows one again', image: Option.none() }] : []),
    ];
};
const attach = Effect.fnUntraced(function* (taken: readonly { readonly image: Option.Option<{ readonly png: Uint8Array; readonly placement: Option.Option<Placement> }> }[]) {
    const [{ add }, handle] = yield* Effect.all([ToolCall, Handle]);
    const images = Array.getSomes(taken.map(Struct.get('image')));
    yield* handle.set(Viewed, Option.flatMap(Array.last(images), Struct.get('placement')));
    yield* Effect.forEach(images, ({ png }) => add(McpSchema.ImageContent.make({ data: png, mimeType: 'image/png' })), { discard: true });
});
const describeTool = <const Name extends string, Fields extends Schema.Struct.Fields, S extends Schema.Top>(tool: Tool.Tool<Name, { readonly parameters: Schema.Struct<Fields>; readonly success: S; readonly failure: Schema.Never; readonly failureMode: 'error' }>, description = tool.description) =>
    Tool.make(tool.name, { description, parameters: tool.parametersSchema.annotate({ messageUnexpectedKey: `is not a parameter of ${tool.name}, which takes ${Outline.list(Struct.keys(tool.parametersSchema.fields))}` }), success: tool.successSchema })
        .annotateMerge(tool.annotations)
        .annotate(Tool.Strict, true);
const sameBundle = (left: Installation, right: Installation) => left.url.href === right.url.href;
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
const scopeOf = (start: Option.Option<Item>, whole: boolean) =>
    Match.type<Item>().pipe(
        Match.when({ target: { _tag: 'Container' } }, ({ target: { container } }) => Result.succeedSome<typeof Scope.Type>({ _tag: 'container', ref: container.ref })),
        Match.when({ target: { _tag: 'Control', control: { kind: 'multi' } } }, ({ target: { control } }) => {
            const from = pipe(
                Option.flatMap(start, ({ target }) => (target._tag === 'Control' ? Option.fromUndefinedOr(target.control.index) : Option.none())),
                Option.match({ onNone: () => 0, onSome: (index) => index + 1 }),
            );
            return Result.succeedSome<typeof Scope.Type>({ _tag: 'rows', ref: control.ref, from, count: whole ? Math.max(1, (control.total ?? Outline.rows) - from) : Outline.rows });
        }),
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
    const given = [...Array.fromNullishOr(to).map((value) => `to ${measured(value)}`), ...Array.fromNullishOr(range).map((value) => `range ${measured(value)}`)].join(' and ');
    return Option.filter(Option.some(`${name} takes ${takes}, not ${given}`), () => !Option.getOrElse(taken, Function.constFalse));
};
const measured = (value: unknown) => (Predicate.isString(value) ? `text of ${value.length} characters` : JSON.stringify(value));
const where = (at: string | Sample) => (Predicate.isString(at) ? at : Array.take(at, 2).join(','));
const labelOf = (step: Step): Phrase =>
    Match.value(step).pipe(
        Match.when({ pointer: Match.defined }, ({ pointer, button = 'left', by, path = [], count = 1 }) => ({
            verb: Match.value({ button, scrolled: by !== undefined, dragged: Array.isReadonlyArrayNonEmpty(path), count }).pipe(
                Match.when({ scrolled: true }, () => 'scroll' as const),
                Match.when({ dragged: true }, () => 'drag' as const),
                Match.when({ button: 'none' }, () => 'hover' as const),
                Match.when({ button: 'right' }, () => 'rightClick' as const),
                Match.when({ button: 'middle' }, () => 'middleClick' as const),
                Match.when({ count: 2 }, () => 'doubleClick' as const),
                Match.orElse(() => 'click' as const),
            ),
            object: [Predicate.isString(pointer) ? pointer : `at ${where(pointer)}`, ...Option.toArray(Option.map(Array.last(path), (end) => `to ${where(end)}`)), ...Array.fromNullishOr(by).map((distance) => `by ${distance.join(',')}`)].join(' '),
        })),
        Match.when({ key: Match.defined }, ({ key, modifiers, at }) => ({ verb: 'press' as const, object: [Outline.keys({ key, modifiers }), ...Array.fromNullishOr(at).map((target) => `in ${target}`)].join(' ') })),
        Match.orElse(({ at, action, to, range }) =>
            Match.value({ action, to, range }).pipe(
                Match.when({ action: Match.string }, ({ action: named }) => ({ verb: 'run' as const, object: `${named} on ${at}` })),
                Match.when({ to: Match.defined }, ({ to: value }) => ({ verb: 'set' as const, object: `${at} to ${measured(value)}` })),
                Match.when({ range: Match.defined }, () => ({ verb: 'select' as const, object: at })),
                Match.orElse(() => ({ verb: 'press' as const, object: at })),
            ),
        ),
    );
const done = ({ verb, object }: Phrase) => `${VERBS[verb][0]} ${object}`;
const provingOf =
    (instance: string) =>
    ([{ label }, outcome]: readonly [{ readonly label: Phrase }, Acted['outcomes'][number]]) =>
        Result.match(outcome, { onFailure: Array.empty, onSuccess: ({ followed }) => Array.fromNullishOr(followed).map((held) => [`${instance} ${held}`, done(label)] as const) });
const unseen = ({ target, token }: Item) => (target._tag === 'Control' && target.control.frame === undefined && target.container.frame !== undefined ? Option.map(token, (at) => `${at} ${Outline.say.unseen} - act {steps:[{pointer:${JSON.stringify(at)}, button:"none"}]}`) : Option.none());
const aimOf = (step: object) => Option.filter(Option.firstSomeOf(['pointer' in step ? Option.some(step.pointer) : Option.none(), 'at' in step ? Option.fromUndefinedOr(step.at) : Option.none()]), Predicate.isNumber);
const describeOutcome = (phrase: Phrase, read: Index) =>
    Result.match({
        onFailure: ({ message }: Error) => `${VERBS[phrase.verb][1]} ${phrase.object} failed: ${message}`,
        onSuccess: ({ proof, route, presentation, popUps, tried, scrolled, followed, resolved }: Delivery) =>
            `${done(phrase)}: ${[
                ...Option.toArray(Option.filter(Option.fromUndefinedOr(scrolled), () => phrase.verb !== 'scroll')).flatMap((held) => Array.match(Outline.scrolled(held.vertical, held.horizontal), { onEmpty: Array.empty, onNonEmpty: (moved) => [`${moved.join(', ')} first`] })),
                Option.match(Option.fromUndefinedOr(resolved), {
                    onNone: () => PROOFS[proof],
                    onSome: ({ ref, effect }) => [effect, ...Option.match(Record.get(read.live, `${ref}`), { onNone: Array.empty, onSome: (item) => [...Option.toArray(item.token), ...Option.toArray(Option.map(item.name, Outline.named))] })].join(' '),
                }),
                ...(proof === 'posting'
                    ? [
                          Option.getOrElse(
                              Option.map(
                                  Array.findFirst(tried, ({ rung }) => rung === route?._tag),
                                  Struct.get('failure'),
                              ),
                              () => 'nothing shown changed',
                          ),
                          followed === undefined ? 'so it may not have landed' : Outline.say.following,
                      ]
                    : []),
                ...Option.toArray(Option.filter(Option.fromUndefinedOr(scrolled), () => phrase.verb === 'scroll')).flatMap((held) => [...Outline.scrolled(held.vertical, held.horizontal), ...Outline.showing(held)]),
                ...(route?._tag === 'source' ? [[`drawn by ${route.member}`, ...Array.match(route.approximated, { onEmpty: Array.empty, onNonEmpty: (lost) => [`without ${Outline.list(lost)}`] })].join(', ')] : []),
                ...(route?._tag === 'focused' ? ['with its window focused'] : []),
                ...presented(presentation),
                ...Record.collect(
                    Record.filter({ closed: Array.countBy(popUps, Struct.get('closed')), 'left open': Array.countBy(popUps, ({ closed }) => !closed) }, (amount) => amount > 0),
                    (left, amount) => `${Outline.count(amount, 'popUp')} ${left}`,
                ),
            ].join(', ')}`,
    });
const diagnose = (application: string, state: Acted['state'], { usage, load }: Pick<Acted, 'usage' | 'load'>) => {
    const running = InstanceState.matchOrElse(state, { running: ({ pendingEventAge: _, ...activity }) => Option.some(activity) }, Option.none);
    const parts = [
        ...Option.toArray(Option.map(Option.all({ activity: running, interval: Option.flatMap(Option.fromUndefinedOr(usage), Result.getSuccess) }), ({ activity, interval }) => `${application}${describeUsage({ ...activity, usage: interval })}`)),
        ...Option.toArray(Option.flatMap(Option.fromUndefinedOr(load), Result.getSuccess)).flatMap(({ idle, swapins = 0, swapouts = 0 }) => [...Array.fromNullishOr(idle).map((share) => `processors ${Outline.percent(1 - share)} busy`), ...(swapins + swapouts > 0 ? [`${swapins + swapouts} pages swapped`] : [])]),
    ];
    return Array.match(parts, { onEmpty: Array.empty, onNonEmpty: (held) => [held.join(', ')] });
};
const itemOf = (base: Index, map: InterfaceMap) => (at: string) => Effect.fromResult(Result.orElse(Address.resolve(base, at), () => Address.resolve(Address.recall(base)(map), at)));
const prepared = <E, F>(instance: InstanceSnapshot['instance'], { base, map, route, find, refOf }: { readonly base: Index; readonly map: InterfaceMap; readonly route: Route; readonly find: (at: string) => Effect.Effect<Item, E>; readonly refOf: (item: Item) => Effect.Effect<number, F> }) => {
    const reach = (at: string) => Effect.flatMap(find(at), refOf);
    const viewed = Effect.map(
        Effect.flatMap(Handle, (handle) => handle.get(Viewed)),
        Option.filter((placement: Placement) => placement.instance === instance),
    );
    const aim = (at: string | Sample): Effect.Effect<number | Sample, E | F | Cause.IllegalArgumentError> => (Predicate.isString(at) ? Effect.map(reach(at), (ref): number | Sample => ref) : Effect.flatMap(viewed, (held) => Effect.fromResult(pointOf(held, at))));
    const plain = { at: Option.none<Option.Option.Value<ReturnType<typeof Address.locate>>>(), start: 'accessibility' as typeof Act.Rung.Type, opener: Option.none<number>(), openers: [], facet: Option.none<typeof Description.Operation.Type>() };
    const canvases = Array.filterMap(base.items, ({ target }) =>
        target._tag === 'Control' && target.control.kind === 'region' ? Result.fromOption(Option.all({ ref: Option.some(target.control.ref), locator: route(target.control.ref), frame: Option.fromUndefinedOr(target.control.frame) }), Function.constVoid) : Result.failVoid,
    );
    const canvasAt = (pointer: number | Sample) =>
        Predicate.isNumber(pointer)
            ? Option.map(
                  Array.findFirst(canvases, ({ ref }) => ref === pointer),
                  ({ locator, frame: [[left, top], [width, height]] }): { readonly locator: Description['containers'][number]['locator']; readonly pointer: Sample } => ({ locator, pointer: [left + width / 2, top + height / 2] }),
              )
            : Option.map(
                  Array.findFirst(canvases, ({ frame: [[left, top], [width, height]] }) => pointer[0] >= left && pointer[0] < left + width && pointer[1] >= top && pointer[1] < top + height),
                  ({ locator }) => ({ locator, pointer }),
              );
    return (step: Step) =>
        Match.value(step).pipe(
            Match.when({ pointer: Match.defined }, (pointing) =>
                Effect.map(Effect.all([aim(pointing.pointer), Effect.forEach(pointing.path ?? [], aim, { concurrency: 'unbounded' }), viewed], { concurrency: 'unbounded' }), ([aimed, path, held]) => {
                    const landed = canvasAt(aimed);
                    const pointer = Option.match(landed, { onNone: () => aimed, onSome: Struct.get('pointer') });
                    return {
                        ...plain,
                        facet: Array.every(path, (point): point is Sample => !Predicate.isNumber(point)) ? Option.map(landed, ({ locator, pointer: start }): typeof Description.Operation.Type => ({ locator, input: { ...pointing, pointer: start, path } })) : Option.none(),
                        step: { ...pointing, pointer, path, ...([pointer, ...path].some(Predicate.not(Predicate.isNumber)) ? Option.getOrUndefined(Option.map(held, Struct.pick(['windows']))) : {}) },
                    };
                }),
            ),
            Match.when({ key: Match.defined }, ({ at, ...keyed }) => Effect.map(Effect.transposeOption(Option.map(Option.fromUndefinedOr(at), reach)), Option.match({ onNone: () => ({ ...plain, step: keyed }), onSome: (ref) => ({ ...plain, step: { ...keyed, at: ref } }) }))),
            Match.orElse((pressing) =>
                Effect.gen(function* () {
                    const item = yield* find(pressing.at);
                    yield* Option.match(accepts(item, pressing), { onNone: () => Effect.void, onSome: (refusal) => new Cause.IllegalArgumentError(refusal) });
                    const opening = item.live ? [] : yield* Effect.fromResult(Result.mapError(Address.opening(Address.recall(base)(map), item), (refused) => new Cause.IllegalArgumentError(refused)));
                    const [ref, refs] = yield* Effect.all([refOf(item), Effect.forEach(opening, refOf, { concurrency: 'unbounded' })], { concurrency: 'unbounded' });
                    const located = Address.locate(item);
                    const from = Option.filter(Option.flatMap(located, map.route.bind(map)), (rung): rung is Exclude<typeof Act.Rung.Type, 'source'> => rung !== 'source');
                    const to = Option.getOrUndefined(Option.map(Option.filter(Option.fromUndefinedOr(pressing.to), Schema.is(Value)), (value) => ({ to: value })));
                    return {
                        ...plain,
                        at: located,
                        start: Option.getOrElse(from, () => plain.start),
                        openers: Array.zipWith(opening, refs, (opener, at) => ({
                            ...plain,
                            label: { verb: 'press' as const, object: `${Option.getOrElse(opener.token, Function.constant(Address.path(opener).join(Address.separator)))} to reach ${pressing.at}` },
                            opener: Option.some(at),
                            at: Address.locate(opener),
                            step: { at },
                        })),
                        facet: Option.map(route(ref), (locator): typeof Description.Operation.Type => ({ locator, ...to })),
                        step: { ...pressing, at: ref, ...Option.getOrUndefined(Option.map(from, (rung) => ({ from: rung }))) },
                    };
                }),
            ),
            Effect.map((entry) => ({ ...entry, label: labelOf(step) })),
        );
};
const printEntry = <A extends Readonly<Record<string, unknown>>>(record: A, lead: readonly (keyof A & string)[]) => [...lead.map((key) => record[key]).filter(Predicate.isString), ...Record.collect(Struct.omit(record, lead), (key, value) => `${key}=${JSON.stringify(value)}`)].join(' ').replaceAll('\n', '\n    ');
const logged = (file: Option.Option<string>, timestamp: Option.Option<string>, text: string): Logged => {
    const pieces = String.split(text, NUMBER);
    return { file, timestamp, pieces, key: pieces.filter((_, at) => at % 2 === 0).join('#') };
};
const coalesce = (entries: readonly Logged[]) =>
    (Array.isReadonlyArrayNonEmpty(entries) ? Array.groupWith(entries, (left, right) => left.key === right.key && Equal.equals(left.file, right.file)) : []).map((run) => {
        const [newest, oldest] = [Array.headNonEmpty(run), Array.lastNonEmpty(run)];
        const text = newest.pieces
            .map((piece, at) => {
                const values = Array.map(run, ({ pieces }) => globalThis.Number(pieces[at] ?? piece));
                return at % 2 === 0 || Array.dedupe(values).length === 1 ? piece : `${Array.min(values, Order.Number)} to ${Array.max(values, Order.Number)}`;
            })
            .join('');
        const span = Predicate.isTupleOf(run, 1) ? newest.timestamp : Option.map(Option.all([oldest.timestamp, newest.timestamp]), ([from, to]) => `${from} to ${to}`);
        return { file: newest.file, line: [...Option.toArray(span), ...(run.length > 1 ? [`${run.length} times`] : []), text].join(' ') };
    });
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
} = {}): Effect.fn.Return<void, never, ApplicationServer | Workspace | InterfaceMaps | Output | McpServer.McpServer | Context.Service.Identifier<typeof Side> | Exclude<R1 | R2 | R3 | R4 | R5, McpSchema.McpRequestContext>> {
    const [server, workspace, maps, side, output] = yield* Effect.all([ApplicationServer, Workspace, InterfaceMaps, Side, Output]);
    const { catalog, running, changes, session, pressure, target, setTarget, resolve, current, ends } = workspace;
    const routed = yield* Ref.make(Record.empty<string, Record.ReadonlyRecord<string, Description['containers'][number]['locator']>>());
    const linked = (row: Entry) => Effect.flatMap(Effect.fromNullishOr(sourceOf), (from) => from(row));
    const routeOf = (instance: InstanceSnapshot['instance']) =>
        Effect.map(
            Ref.get(routed),
            (held): Route =>
                (ref) =>
                    Option.flatMap(Record.get(held, instance), Record.get(`${ref}`)),
        );
    const interning = RequestResolver.makeGrouped<Interned, InstanceSnapshot['instance']>({
        key: ({ request }) => request.instance,
        resolver: (entries, instance) => Effect.flatMap(server.intern({ instance, locators: entries.map(({ request }) => request.locator) }), (refs) => Effect.all(Array.zipWith(entries, refs, Request.succeed), { discard: true })),
    });
    const intern = (instance: InstanceSnapshot['instance'], locator: string) => Effect.request(new Interned({ instance, locator }), interning);
    const refOf = (instance: InstanceSnapshot['instance']) => (item: Item) =>
        Address.Target.$match(item.target, {
            Container: ({ container }) => Effect.succeed(container.ref),
            Control: ({ control }) => Effect.succeed(control.ref),
            Option: ({ option }) => Effect.succeed(option.ref),
            RememberedContainer: ({ locator }) => intern(instance, locator),
            RememberedControl: ({ locator }) => intern(instance, locator),
        });
    const handlersOf = (urls: readonly URL[]) => Effect.forEach(urls, (document) => Effect.map(server.handlers({ document }), Struct.get('installations')), { concurrency: 'unbounded' });
    const listed = Effect.fnUntraced(function* (name: string) {
        const row = yield* workspace.find(name);
        const [installation, dictionary, held, runs, { held: aim }, now] = yield* Effect.all([
            server.installation({ url: row.url }),
            Effect.option(Effect.tapError(server.dictionary({ url: row.url }), (cause) => Effect.logDebug('Scripting dictionary unread', cause))),
            Effect.option(Effect.tapError(maps.held(row.identifier), (cause) => Effect.logWarning('Interface map unread', cause))),
            workspace.instancesOf,
            target,
            DateTime.now,
        ]);
        const targeted = Option.map(aim, Struct.get('instance'));
        const instances = yield* Effect.forEach(runs(row).meant, (each) => (Option.contains(targeted, each.instance) ? server.watch({ instance: each.instance }) : Effect.succeed(each)), { concurrency: 'unbounded' });
        const { head, rest } = describeInstallation(installation, Option.flatMap(dictionary, Struct.get('open')));
        const languages = Option.getOrElse(held, Record.empty);
        const several = Record.size(languages) > 1;
        return [
            [...describeRow(row), ...head].join(', '),
            ...rest,
            ...Record.collect(languages, (language, map) => describeCoverage(several ? Option.some(language) : Option.none(), map.coverage)),
            ...instances.map((each) => ['Running', ...describeState(each.state), ...describeInstance(row, Option.some(now))(each)].join(', ')),
        ];
    });
    const list = Effect.fnUntraced(function* (given: string | undefined, documents: readonly string[] = []) {
        const [entries, { held }, opening, runs] = yield* Effect.all([SubscriptionRef.get(catalog), target, handlersOf(documents.map(locate)), workspace.instancesOf]);
        const repeated = new Set(Record.keys(Record.filter(Array.groupBy(entries, Struct.get('name')), (alike) => alike.length > 1)));
        const opens = Array.groupBy(Array.flatten(Array.zipWith(opening, documents, (found, document) => Array.cartesian(found, [document]))), ([{ url }]) => url.href);
        const line = (row: Entry, opened: readonly string[]) =>
            [
                ...describeRow(row),
                ...(repeated.has(row.name) ? [`at ${fileURLToPath(row.url)}`] : []),
                ...Option.match(Array.head(runs(row).meant), { onNone: Array.empty, onSome: (each) => ['running', ...describeState(each.state), ...describeInstance(row, Option.none())(each)] }),
                ...Array.match(opened, { onEmpty: Array.empty, onNonEmpty: (named) => [`opens ${Outline.list(named)}`] }),
                ...(Option.exists(held, (aim) => aim.row.url.href === row.url.href) ? ['the target'] : []),
            ].join(', ');
        const lines = yield* UndefinedOr.match(given, {
            onDefined: listed,
            onUndefined: () =>
                Effect.succeed(
                    Array.match(documents, {
                        onEmpty: () => entries.map((row) => line(row, [])),
                        onNonEmpty: () =>
                            Record.values(
                                Record.intersection(
                                    opens,
                                    Record.fromIterableBy(entries, (row) => row.url.href),
                                    (pairs, row) => line(row, pairs.map(Tuple.get(1))),
                                ),
                            ),
                    }),
                ),
        });
        return Array.match(lines, {
            onEmpty: () => Array.match(documents, { onEmpty: () => 'No application is installed', onNonEmpty: (named) => `No listed application opens ${Outline.list(named)}` }),
            onNonEmpty: (rows) => rows.join('\n'),
        });
    });
    const open = Effect.fnUntraced(function* (given: string | undefined, { documents = [], wait, activate = false, ...launch }: Omit<Tool.Parameters<typeof TOOLS.applications>, 'action' | 'application' | 'mode'>) {
        const urls = documents.map(locate);
        const handler = Effect.gen(function* () {
            const opening = yield* handlersOf(urls);
            const defaults = Array.dedupeWith(Array.getSomes(opening.map(Array.head)), sameBundle);
            const every = Array.reduce(Array.drop(opening, 1), Array.flatten(Array.take(opening, 1)), Array.intersectionWith(sameBundle));
            const shared = Array.match(every, { onEmpty: () => 'no application opens every one', onNonEmpty: (found) => `${Outline.list(found.map(Struct.get('name')))} open every one` });
            const refusal = Array.match(defaults, { onEmpty: () => 'open takes an application from the list, or documents the system has an application for', onNonEmpty: (found) => `${Outline.list(found.map(Struct.get('name')))} open these documents by default and ${shared}, application names the one to open` });
            return Predicate.isTupleOf(defaults, 1) ? defaults[0].url.href : yield* new Cause.IllegalArgumentError(refusal);
        });
        const row = yield* Effect.flatMap(UndefinedOr.match(given, { onUndefined: () => handler, onDefined: Effect.succeed }), resolve);
        const { instance, created, replaced, opened, failed } = yield* server.runLaunch({ url: row.url, documents: urls, ...launch }, Option.map(Option.fromUndefinedOr(wait), Duration.seconds));
        const live = yield* setTarget({ row, instance });
        const front = activate ? Option.some(yield* server.activate({ instance })) : Option.none();
        const [reply, read] = yield* Effect.all([Effect.transposeOption(Option.map(opened, Schema.encodeEffect(Reply))), Effect.result(modelled({ row, live }))]);
        const shown = yield* Result.match(read, {
            onFailure: ({ message }) => Effect.succeed([`Its interface could not be read: ${message}`]),
            onSuccess: ({ base, indexed }) =>
                Array.match(
                    base.model.containers.filter(({ kind }) => kind !== 'menuBar' && kind !== 'menu'),
                    { onEmpty: () => Effect.map(unshown({ row, live }, Effect.succeed(base)), Array.of), onNonEmpty: (containers) => Effect.fromResult(Outline.render(indexed(containers))) },
                ),
        });
        yield* stated(row.name, live.state);
        return [
            [
                ...Option.match(front, { onNone: () => [created ? 'Launched behind' : 'Reopened behind'], onSome: (held) => [created ? 'Launched' : 'Reopened', held._tag === 'forward' ? Outline.say.front : Outline.say.unfronted] }),
                ...(live.url.href === row.url.href ? [] : [`from ${fileURLToPath(live.url)}`]),
                ...(replaced === true ? ['its code changed on disk since it started'] : []),
                ...Option.toArray(reply).map((value) => `the open command answered ${JSON.stringify(value)}`),
            ].join(', '),
            ...failed.map(({ subject, reason, message }) => [`After launch, ${subject} failed, ${reason}`, ...Array.fromNullishOr(message)].join(': ')),
            ...shown,
        ].join('\n');
    });
    const close = Effect.fnUntraced(function* (given: string | undefined, { wait, ...quitting }: Pick<Tool.Parameters<typeof TOOLS.applications>, 'mode' | 'wait'>) {
        const [instances, { followed }, chosen, runs] = yield* Effect.all([SubscriptionRef.get(running), target, Effect.transposeOption(Option.map(Option.fromUndefinedOr(given), resolve)), workspace.instancesOf]);
        const found = Option.map(chosen, runs);
        const quit = Option.match(found, { onNone: () => instances.filter((each) => HashSet.has(followed, each.instance) && !InstanceState.isAnyOf(['exited'])(each.state)), onSome: Struct.get('meant') });
        const apart = Option.map(
            Option.flatMap(found, ({ apart: others }) => Array.head(others)),
            ({ url }) => fileURLToPath(url),
        );
        const refusal = Option.match(chosen, {
            onNone: () => 'No application this handle opened runs, application names the one to close',
            onSome: ({ name }) => Option.match(apart, { onNone: () => `${name} does not run`, onSome: (path) => `${name} runs from ${path}, which this handle never opened, applications {action:"close", application:${JSON.stringify(path)}} closes it` }),
        });
        yield* Array.isReadonlyArrayEmpty(quit) ? new Cause.NoSuchElementError(refusal) : Effect.void;
        const home = Option.getOrUndefined(Option.map(chosen, ({ url }) => url.href));
        const bound = UndefinedOr.match(wait, { onUndefined: () => Duration.infinity, onDefined: Duration.seconds });
        yield* Effect.forEach(quit, (each) => follow(each.instance, true), { discard: true });
        const outcomes = yield* Effect.forEach(quit, (each) => Effect.result(Effect.timeoutOption(server.quit({ instance: each.instance, ...quitting }), bound)), { concurrency: 'unbounded' });
        return Array.zipWith(quit, outcomes, (each, outcome) =>
            [
                [instanceName(each), ...(home === undefined || home === each.url.href ? [] : [`from ${fileURLToPath(each.url)}`])].join(' '),
                Result.match(outcome, {
                    onFailure: (error) => (Schema.is(ApplicationFailure)(error) ? `did not quit: ${error.message}` : `quit unconfirmed: ${error.message}`),
                    onSuccess: Option.match({
                        onNone: () => `still runs after ${Outline.duration(bound)}`,
                        onSome: InstanceState.match({
                            exited: () => 'quit',
                            running: ({ windows }) => (windows.some(Struct.get('asking')) ? `still runs, its dialog ${Outline.say.asking}` : 'still runs'),
                            failed: (state) => describeState(state).join(', '),
                        }),
                    }),
                }),
            ].join(' '),
        ).join('\n');
    });
    const facets = Effect.fnUntraced(function* ({ row, live, live: { instance } }: Effect.Success<typeof current>, from: Linked, model: Model, payload: { readonly within?: Description['containers'][number]['locator'] }) {
        const [described, tools] = yield* Effect.all([from('describe', payload), toolsOf(row)]);
        const { model: composed, routes } = yield* Description.compose(model, yield* maps.learn(live, Description.fold(described, tools)), (locators) => server.intern({ instance, locators }));
        yield* Ref.update(routed, (held) => Record.set(held, instance, { ...held[instance], ...routes }));
        return composed;
    });
    const modelled = Effect.fnUntraced(function* ({ row, live, live: { instance } }: Effect.Success<typeof current>) {
        const map = yield* maps.recall(live);
        const read = (scope: typeof Scope.Type) => server.model({ instance, scope });
        const [accessible, menus, client] = yield* Effect.all(
            [
                server.model({
                    instance,
                    scope: { _tag: 'application' },
                    ...Option.getOrUndefined(Option.map(map.known, (known) => ({ known }))),
                }),
                read({ _tag: 'menus' }),
                Effect.option(linked(row)),
            ],
            { concurrency: 'unbounded' },
        );
        const application: Model = yield* Option.match(client, { onNone: () => Effect.succeed(accessible), onSome: (from) => facets({ row, live }, from, accessible, {}) });
        yield* maps.sight(live, application.pointer);
        const [route, handle] = yield* Effect.all([routeOf(instance), Handle]);
        const indexed = (containers: Model['containers']) => Address.index({ ...application, containers });
        const base = indexed(Array.unionWith(application.containers, menus.containers, (left, right) => left.ref === right.ref));
        yield* handle.set(Indexed, Option.some({ instance, index: base }));
        const level = (scope: typeof Scope.Type) =>
            Option.match(Option.all({ from: client, within: Option.flatMap(Scope.matchOrElse(scope, { container: ({ ref }) => Option.some(ref), menu: ({ ref }) => Option.some(ref), rows: ({ ref }) => Option.some(ref) }, Option.none<number>), route) }), {
                onNone: () => read(scope),
                onSome: ({ from, within }) => facets({ row, live }, from, { sequence: application.sequence, containers: [] }, { within }),
            });
        return { row, live, application, map, route, indexed, level, base, handle };
    });
    const clipped = (instance: InstanceSnapshot['instance'], asked: boolean) => (pasteboard: Pasteboard) => Effect.map(intern(instance, pasteboard.locator), (ref) => Outline.clipboard(pasteboard, ref, asked));
    const stated = (application: string, state: InstanceSnapshot['state'], explains: Explains = {}) => Effect.flatMap(Effect.all([session, pressure]), ([screen, strain]) => ToolCall.use((call) => call.state(stateLine(application, state, { screen, strain, ...explains }))));
    const capture = Effect.fnUntraced(function* (application: Option.Option<string>, snapshot: InstanceSnapshot, source: Capture['source'], drawn: Drawn) {
        const { instance } = snapshot;
        const marks = Option.match(
            Option.filter(drawn, (held) => held.instance === instance),
            { onNone: Array.empty, onSome: Struct.get('marks') },
        );
        const shots = yield* Option.match(yield* Clip, {
            onNone: () =>
                Effect.map(
                    Effect.tap(server.capture({ instance, source, longestSide: Option.some(side), marks }), (results) =>
                        Effect.when(new Cause.NoSuchElementError(results.flatMap(Result.match({ onFailure: ({ message }) => [[...Option.toArray(application), message].join(': ')], onSuccess: Array.empty })).join('\n')), Effect.succeed(results.every(Result.isFailure))),
                    ),
                    (results) => (rows: Rows) => results.map(Result.match({ onFailure: ({ message }) => ({ line: [...Option.toArray(application), message].join(': '), image: Option.none() }), onSuccess: shot(application, instance, rows, []) })),
                ),
            onSome: (clip) => Effect.map(server.record({ instance, source, longestSide: Option.some(side), ...clip }), (recording) => (rows: Rows) => keyFramesOf(application, instance, rows, recording)),
        });
        const instances = yield* SubscriptionRef.get(running);
        return Array.findFirst(instances, (each) => each.instance === instance).pipe(Option.match({ onNone: () => snapshot.state, onSome: (each) => each.state }), windowsOf, shots);
    });
    const unshown = <E, R>({ row, live }: Effect.Success<typeof current>, read: Effect.Effect<Index, E, R>) => Effect.map(Effect.all({ index: read, installation: server.installation({ url: row.url }) }), ({ index, installation }) => unshownOf(row, live.state, index, installation));
    const refuseUnshown = <E, R>(aim: Effect.Success<typeof current>, read: Effect.Effect<Index, E, R>) => Effect.flatMap(unshown(aim, read), (refusal) => new Cause.NoSuchElementError(refusal));
    const frontWindow = <E, R>(aim: Effect.Success<typeof current>, read: Effect.Effect<Index, E, R>) => Option.match(frontOf(aim.live.state), { onSome: Effect.succeed, onNone: () => refuseUnshown(aim, read) });
    const zoom = Effect.fnUntraced(function* (pixels: typeof Pixels.Type, drawn: Drawn) {
        const [viewed, instances] = yield* Effect.all([Effect.flatMap(Handle, (handle) => handle.get(Viewed)), SubscriptionRef.get(running)]);
        const placement = yield* Effect.fromOption(viewed, () => new Cause.NoSuchElementError('view of a pixel rectangle reads the last image a view returned, and this handle has none placed on the screen'));
        const each = yield* Effect.fromOption(
            Array.findFirst(instances, (held) => held.instance === placement.instance),
            () => new Cause.NoSuchElementError('The application of the last image a view returned no longer runs, view it again'),
        );
        const frame = yield* Effect.fromResult(screenOf(placement, pixels));
        return yield* capture(Option.none(), each, { _tag: 'region', frame }, drawn);
    });
    const element = (reference: string, drawn: Drawn) =>
        Effect.fnUntraced(function* ({ row, live, map, route, indexed, level, base }: Effect.Success<ReturnType<typeof modelled>>) {
            const item = yield* itemOf(base, map)(reference);
            yield* Option.match(unseen(item), { onNone: () => Effect.void, onSome: (line) => new Cause.NoSuchElementError(line) });
            const ref = yield* refOf(live.instance)(item);
            const named = Outline.named(Address.path(item).join(Address.separator));
            const recalled = item.live ? Option.none<Index>() : map.pipe(Address.recall(base), Option.some);
            const holder = Option.flatMap(recalled, ({ located }) => (item.target._tag === 'RememberedControl' ? Option.map(Record.get(located, item.target.container), Array.headNonEmpty) : Option.some(item)));
            const placed = Match.value(item.target).pipe(
                Match.tag('Control', ({ control, container }) => ({ frame: control.frame ?? container.frame, window: container.window })),
                Match.tag('Container', ({ container }) => ({ frame: container.frame, window: container.window })),
                Match.orElse(() => ({ frame: undefined, window: undefined })),
            );
            const source: Capture['source'] = Option.isNone(route(ref))
                ? { _tag: 'element', ref }
                : yield* UndefinedOr.match(placed.frame, {
                      onDefined: (frame) => Effect.succeed<Capture['source']>({ _tag: 'region', frame }),
                      onUndefined: () => Effect.map(UndefinedOr.match(placed.window, { onDefined: Effect.succeed, onUndefined: () => frontWindow({ row, live }, Effect.succeed(base)) }), (number): Capture['source'] => ({ _tag: 'windows', numbers: Array.of(number) })),
                  });
            const shots = item.live ? yield* capture(Option.none(), live, source, drawn) : [{ line: `${named} is not open, so its outline comes from the map, act presses its items by path`, image: Option.none() }];
            const opened = yield* Effect.when(level({ _tag: 'menu', ref }), Effect.succeed(item.target._tag === 'Control' && item.target.control.opens !== undefined));
            const containers = Option.match(opened, { onNone: Array.empty, onSome: Struct.get('containers') });
            const fresh = indexed(containers);
            const levels = [...menuPath(base, item).map((each) => ({ index: base, item: each })), ...Option.toArray(Option.all({ index: recalled, item: holder })), ...Array.getSomes(containers.map((container) => Record.get(fresh.live, `${container.ref}`))).map((each) => ({ index: fresh, item: each }))];
            const outline = yield* Effect.forEach(levels, ({ index, item: each }) => Effect.fromResult(Result.map(Result.all([Outline.hits([each], []), Outline.render(index, { in: Option.some(each) })]), Array.flatten)));
            return [...shots, ...outline.flat().map((line) => ({ line, image: Option.none() }))];
        });
    const front =
        <E, R>(drawn: Drawn, read: Effect.Effect<Index, E, R>) =>
        (aim: Effect.Success<typeof current>) =>
            Effect.flatMap(frontWindow(aim, read), (number) => capture(Option.none(), aim.live, { _tag: 'windows', numbers: Array.of(number) }, drawn));
    const every =
        <E, R>(drawn: Drawn, read: Effect.Effect<Index, E, R>) =>
        (aim: Effect.Success<typeof current>) =>
            Array.match(
                windowsOf(aim.live.state)
                    .filter((window) => window.kind !== 'menu')
                    .map(Struct.get('number')),
                { onEmpty: () => refuseUnshown(aim, read), onNonEmpty: (numbers) => capture(Option.none(), aim.live, { _tag: 'windows', numbers }, drawn) },
            );
    const document = (drawn: Drawn) =>
        Effect.fnUntraced(function* (id: string) {
            const instances = yield* SubscriptionRef.get(running);
            const windowed = showing(instances, id);
            yield* Array.isReadonlyArrayEmpty(windowed) ? new Cause.NoSuchElementError(`No window of the applications this server drives shows document ${id}`) : Effect.void;
            return Array.flatten(yield* Effect.forEach(windowed, ({ each, numbers }) => capture(Option.some(instanceName(each)), each, { _tag: 'windows', numbers }, drawn), { concurrency: 'unbounded' }));
        });
    const object = Effect.fnUntraced(function* ([{ row }, id]: readonly [Pick<Effect.Success<typeof current>, 'row'>, string]) {
        const renderer = yield* Effect.fromOption(Option.fromUndefinedOr(render), () => new Cause.NoSuchElementError(`No linked source renders objects of ${row.name}`));
        const { image, title } = yield* renderer(row, id);
        const { png } = yield* server.scale({ image, longestSide: side });
        return [{ line: `Rendered ${Outline.named(title)}`, image: Option.some({ png, placement: Option.none<Placement>() }) }];
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
        Effect.fnUntraced(function* ({ of = 'front', id, marks = false, seconds, frames }) {
            const identified = (kind: 'document' | 'object') => Effect.fromOption(Option.fromUndefinedOr(id), () => new Cause.IllegalArgumentError(`view of ${kind} takes an id`));
            const clip = Option.filter(
                Option.map(Option.fromUndefinedOr(seconds), (span) => ({ seconds: span, frames })),
                ({ frames: kept }) => kept > 1,
            );
            yield* Option.isSome(clip) && (marks || of === 'object') ? new Cause.IllegalArgumentError('view with seconds records windows the application shows, and marks and objects draw on one image, so view them without seconds') : Effect.void;
            const aimed = yield* Effect.cached(Effect.tap(current, ({ row, live }) => stated(row.name, live.state)));
            const reading = yield* Effect.cached(Effect.flatMap(aimed, modelled));
            const read = Effect.map(reading, Struct.get('base'));
            const drawn: Drawn = marks ? Option.some(yield* Effect.map(reading, ({ live, base }) => ({ instance: live.instance, marks: marksOf(base) }))) : Option.none();
            const taken = yield* Match.value(of).pipe(
                Match.when('front', () => Effect.flatMap(aimed, front(drawn, read))),
                Match.when('all', () => Effect.flatMap(aimed, every(drawn, read))),
                Match.when('document', () => Effect.flatMap(identified('document'), document(drawn))),
                Match.when('object', () => Effect.flatMap(Effect.all([aimed, identified('object')]), object)),
                Match.when(Predicate.isString, (reference) => Effect.flatMap(reading, element(reference, drawn))),
                Match.orElse((pixels) => zoom(pixels, drawn)),
                Effect.provideService(Clip, clip),
            );
            yield* attach(taken);
            return taken.map(Struct.get('line')).join('\n');
        }),
    );
    yield* Server.registerTool(
        describeTool(TOOLS.inspect, descriptions.inspect),
        Effect.fnUntraced(function* ({ in: within, find: query, kind, after, wait, unopened = false }) {
            const aimed = yield* current;
            const { row, live } = aimed;
            if (within === 'clipboard') {
                yield* stated(row.name, live.state);
                return yield* Effect.flatMap(server.pasteboard({ content: true }), clipped(live.instance, true));
            }
            const searched = Record.collect(Record.getSomes({ find: Option.fromUndefinedOr(query), kind: Option.fromUndefinedOr(kind), unopened: Option.liftPredicate<Schema.Json>(unopened, Boolean) }), Outline.quoted);
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
            const changed = pipe(
                first,
                Option.flatMap(({ change }: Event) => (Event.fields.change.isAnyOf(['focused', 'value', 'selection'])(change) ? Option.some(change.control) : Option.none())),
                Option.toArray,
                HashSet.fromIterable,
            );
            const [opening, [elapsed, { application, map, indexed, level, base, handle }], closing] = yield* Effect.all([server.traffic, Effect.timed(modelled(aimed)), server.traffic]);
            yield* whole ? Effect.annotateLogs(Effect.logInfo('Inspect read'), { application: row.identifier, build: Option.getOrElse(map.coverage.build, () => 'unmapped'), calls: closing.requests - opening.requests, bytes: closing.bytes - opening.bytes, milliseconds: Duration.toMillis(elapsed) }) : Effect.void;
            const recalled = Address.recall(base)(map);
            const lookup = (at: string | undefined) =>
                Result.transposeMapOption(Option.fromUndefinedOr(at), (held) => {
                    const resolved = Address.resolve(base, held);
                    return Result.isSuccess(resolved) ? resolved : Address.resolve(recalled, held);
                });
            const [scope, start] = yield* Effect.fromResult(Result.all([lookup(within), lookup(after)]));
            const stored = Option.flatMap(scope, ({ target: pressed }) => (pressed._tag === 'Control' && pressed.control.opens !== undefined && Option.getOrUndefined(Record.get(base.live, `${pressed.control.opens}`))?.target._tag !== 'Container' ? map.level(pressed.control.locator) : Option.none()));
            const shelved = Option.flatMap(stored, ({ locator }) => Option.flatMap(Record.get(recalled.located, locator), Array.findFirst(Predicate.not(Struct.get('live')))));
            const reading = yield* Effect.fromResult(
                Option.match(
                    Option.filter(scope, () => Option.isNone(shelved)),
                    { onNone: () => Result.succeedNone, onSome: scopeOf(start, Array.isReadonlyArrayNonEmpty(searched)) },
                ),
            );
            const fresh = yield* Effect.transposeOption(Option.map(reading, level));
            yield* Option.match(fresh, { onNone: () => Effect.void, onSome: ({ containers }) => handle.set(Indexed, Option.some({ instance: live.instance, index: indexed(Array.unionWith(containers, base.model.containers, (left, right) => left.ref === right.ref)) })) });
            const remembered = Option.isSome(shelved) || [scope, start].some(Option.exists(Predicate.not(Struct.get('live'))));
            const index = Option.match(fresh, { onNone: () => (remembered ? recalled : base), onSome: ({ containers }) => indexed(containers) });
            const rows = Option.exists(reading, Scope.isAnyOf(['rows']));
            const opened = pipe(
                Option.filter(fresh, () => Option.exists(reading, Scope.isAnyOf(['menu']))),
                Option.flatMap(({ containers }) => Array.head(containers)),
                Option.flatMap(({ ref }) => Record.get(index.live, `${ref}`)),
            );
            const { coverage } = map;
            const gaps = Array.match(coverage.openers, { onEmpty: Array.empty, onNonEmpty: (never) => [neverOpened(never.length)] });
            const member = memberOf(index, scope);
            const partial = Option.isSome(scope) || unopened || Array.isReadonlyArrayEmpty(searched) ? [] : partialOf(base);
            const completed = yield* Effect.forEach(
                partial,
                Effect.fnUntraced(function* ({ control, item, loaded }) {
                    const model = yield* Effect.option(level({ _tag: 'rows', ref: control.ref, from: loaded, count: (control.total ?? loaded) - loaded }));
                    return { control, item, loaded, containers: Option.match(model, { onNone: Array.empty, onSome: Struct.get('containers') }) };
                }),
            );
            const merged = Array.isReadonlyArrayEmpty(completed) ? base : indexed(joinedOf(base, completed));
            yield* Array.isReadonlyArrayEmpty(completed) ? Effect.void : handle.set(Indexed, Option.some({ instance: live.instance, index: merged }));
            const unread = completed.flatMap((each) => unreadOf(each, indexed(each.containers)));
            const lines = yield* Effect.fromResult(
                Array.match(searched, {
                    onEmpty: () =>
                        Outline.render(index, {
                            in: Option.orElse(
                                Option.orElse(shelved, () => opened),
                                () => scope,
                            ),
                            after: rows ? Option.none() : start,
                            changed,
                        }),
                    onNonEmpty: (call) => {
                        const searchedIn = Option.isSome(scope) ? index : Address.recall(merged)(map);
                        const openers = coverage.openers.flatMap(({ control }) => Option.getOrElse(Record.get(recalled.located, control), Array.empty));
                        const hits = Address.find(unopened ? { ...recalled, items: openers } : { ...searchedIn, items: searchedIn.items.filter(member) }, query ?? '', Option.fromUndefinedOr(kind));
                        const summary = Match.value({ unopened, scoped: Option.flatMap(scope, Struct.get('token')) }).pipe(
                            Match.when({ unopened: true }, () => [neverOpened(openers.length)]),
                            Match.when({ scoped: Option.isSome }, ({ scoped }) => [`${Outline.count(hits.length, 'match')} in ${scoped.value}`]),
                            Match.orElse(() => [`${Outline.count(hits.length, 'match')} in ${searchedIn.shown.length} open and ${Outline.count(searchedIn.remembered.containers.length, 'container')} remembered`, ...unread, ...(hits.length === 0 ? gaps : [])]),
                        );
                        return Result.map(Outline.hits(hits, call, { after: start, opening: Option.some(recalled) }), Array.appendAll(summary));
                    },
                }),
            );
            const multi = Option.flatMap(reading, Scope.matchOrElse({ rows: ({ ref }) => Option.some(ref) }, Option.none<number>));
            const listedRows = Option.match(multi, { onNone: Array.empty, onSome: (ref) => rowsOf(index, ref) });
            const total = Option.flatMap(scope, (chosen) => (chosen.target._tag === 'Control' ? Option.fromUndefinedOr(chosen.target.control.total) : Option.none()));
            const more = Option.all({
                in: Option.flatMap(scope, Struct.get('token')),
                after: pipe(
                    Array.last(listedRows),
                    Option.filter(({ control }) => Option.match(Option.all({ at: Option.fromUndefinedOr(control.index), total }), { onNone: () => listedRows.length >= Outline.rows, onSome: ({ at, total: held }) => at + 1 < held })),
                    Option.flatMap(({ item }) => item.token),
                ),
            });
            const unloaded = Option.filter(Option.all({ in: Option.flatMap(scope, Struct.get('token')), after: Option.flatMap(start, Struct.get('token')) }), () => rows && Array.isReadonlyArrayEmpty(listedRows));
            const answer = Option.flatMap(
                Array.findFirst(index.shown, ({ container }) => container.asking),
                answerOf,
            );
            const { tool, shortcut, help, holding = [], operations = [], cursor, draggingFrame } = application.pointer ?? {};
            const seenWith = pipe(
                Option.fromUndefinedOr(cursor),
                Option.flatMap(Cursor.match({ system: () => Option.none<string>(), custom: ({ digest }) => Option.some(digest) })),
                Option.flatMap((digest) => map.cursor(digest)),
                Option.map(Record.filter((_, named) => named !== tool)),
                Option.map(Record.keys),
                Option.filter(Array.isReadonlyArrayNonEmpty<string>),
                Option.map((named) => `seen with the tools ${Outline.list(named)}`),
            );
            const pointed = [
                ...Array.fromNullishOr(tool).map((named) => `tool ${named}`),
                ...Array.fromNullishOr(shortcut).map(Outline.shortcut),
                ...(Array.isReadonlyArrayNonEmpty(holding) ? [`holding ${Outline.list(holding)}`] : []),
                ...(Array.isReadonlyArrayNonEmpty(operations) ? [`running ${operations.join(', then ')}`] : []),
                ...Array.fromNullishOr(cursor).map((held) => Cursor.match(held, { system: ({ name }) => `cursor ${name}`, custom: () => ['custom cursor', ...Option.toArray(seenWith)].join(', ') })),
                ...(draggingFrame === undefined ? [] : ['dragging']),
            ];
            yield* stated(row.name, live.state, { pointed, help });
            return [
                ...lines,
                ...(Option.isSome(shelved) ? ['This menu comes from the map of this build and is not open, act presses its items by path with it closed'] : []),
                ...Option.toArray(Option.map(more, (next) => `More rows can follow, ${Outline.call([Outline.quoted('in', next.in), Outline.quoted('after', next.after)])} lists them`)),
                ...Option.toArray(Option.map(unloaded, (held) => `${held.in}, rows after ${held.after} not loaded - ${scrolling(held.in)}`)),
                ...Option.toArray(Option.map(answer, ({ asking, ref }) => `${asking} ${Outline.say.asking}, act {steps:[{at:"${Address.token('action', ref)}"}]} gives it`)),
            ].join('\n');
        }),
    );
    yield* Server.registerTool(
        describeTool(TOOLS.act, descriptions.act),
        Effect.fnUntraced(function* ({ steps, record = false }) {
            const aimed = yield* current;
            const { row, live } = aimed;
            const refs = refOf(live.instance);
            const [last, map, route] = yield* Effect.all([Effect.flatMap(Handle, (handle) => handle.get(Indexed)), maps.recall(live), routeOf(live.instance)], { concurrency: 'unbounded' });
            const prepare = <E>(reads: Parameters<typeof prepared<E, Effect.Error<ReturnType<typeof refs>>>>[1]) =>
                Effect.map(
                    Effect.forEach(steps, (step) => Effect.result(prepared(live.instance, reads)(step)), { concurrency: 'unbounded' }),
                    (results) => ({ base: reads.base, results }),
                );
            const fast = yield* Option.match(
                Option.filter(last, ({ instance }) => instance === live.instance),
                {
                    onNone: () => Effect.succeedNone,
                    onSome: ({ index }) =>
                        Effect.map(
                            prepare({ base: index, map, route, refOf: refs, find: (at) => Effect.fromOption(Option.filter(Result.getSuccess(Address.resolve(index, at)), Struct.get('live'))) }),
                            Option.liftPredicate(({ results }) => results.every(Result.isSuccess)),
                        ),
                },
            );
            const { base, results: resolved } = yield* Option.match(fast, {
                onSome: Effect.succeed,
                onNone: () => Effect.flatMap(modelled(aimed), (read) => prepare({ ...read, refOf: refs, find: itemOf(read.base, read.map) })),
            });
            const refusals = Array.zip(steps, resolved).flatMap(([step, result], index) => (Result.isFailure(result) ? [`step ${index + 1}, ${VERBS[labelOf(step).verb][2]} ${labelOf(step).object}: ${result.failure.message}`] : []));
            yield* Array.match(refusals, { onEmpty: () => Effect.void, onNonEmpty: (refused) => new Cause.IllegalArgumentError(`No step ran: ${refused.join('; ')}`) });
            const prepares = yield* Effect.forEach(resolved, Effect.fromResult);
            const ready = Array.dedupeWith(
                Array.flatMap(prepares, (entry) => Array.append(entry.openers, entry)),
                (first, second) => Option.isSome(first.opener) && Equal.equals(first.opener, second.opener),
            );
            const operated = Array.getSomes(ready.map(Struct.get('facet')));
            const client = yield* Effect.cached(linked(row));
            const first = Option.flatMap(Array.head(ready), ({ step }) => aimOf(step));
            const around = Option.firstSomeOf([Option.map(frontOf(live.state), (number): Capture['source'] => ({ _tag: 'windows', numbers: Array.of(number) })), Option.map(first, (ref): Capture['source'] => ({ _tag: 'element', ref }))]);
            const recording: Option.Option<Result.Result<Capture['source'], string>> = Match.value({ record, linked: Array.isReadonlyArrayNonEmpty(operated) }).pipe(
                Match.when({ record: false }, () => Option.none()),
                Match.when({ linked: true }, () => Option.some(Result.fail('Not recorded: a linked source runs a step through its host, which no recording covers'))),
                Match.orElse(() => Option.some(Result.fromOption(around, Function.constant(`Not recorded: ${row.name} showed no window as the steps began, and step 1 aims at no control`)))),
            );
            const recorded = Option.getOrUndefined(Option.map(Option.flatMap(recording, Result.getSuccess), (source) => ({ record: { source, longestSide: Option.some(side), frames: FRAMES } })));
            const sent = Effect.fnUntraced(function* (entries: Array.NonEmptyReadonlyArray<(typeof ready)[number]>) {
                const acted = yield* server.act({ instance: live.instance, steps: Array.map(entries, Struct.get('step')), ...recorded });
                return { entries, outcomes: acted.outcomes, acted: Option.some(acted) } satisfies Run<(typeof ready)[number]>;
            });
            const operate = Effect.fnUntraced(function* (entry: (typeof ready)[number]) {
                const { facet } = entry;
                if (Option.isNone(facet)) {
                    return yield* sent([entry]);
                }
                const reached = yield* Effect.result(client);
                const answered = Result.isFailure(reached) ? Result.fail(reached.failure) : yield* Effect.result(reached.success('operate', facet.value));
                if (Result.isSuccess(answered)) {
                    return { entries: [], outcomes: [Result.succeed({ proof: 'answer' as const, ...(Schema.is(Act.Route)(answered.success) ? { route: answered.success } : {}), tried: [], popUps: [] })], acted: Option.none() };
                }
                const refused = { rung: 'source' as const, failure: answered.failure.message };
                if (!('input' in facet.value)) {
                    return { entries: [], outcomes: [Result.fail(answered.failure)], acted: Option.none() };
                }
                const fallen = yield* sent([entry]);
                return {
                    ...fallen,
                    outcomes: Array.map(
                        fallen.outcomes,
                        Result.mapBoth({
                            onSuccess: (delivery) => ({ ...delivery, tried: [refused, ...delivery.tried] }),
                            onFailure: (cause) => new Cause.UnknownError(cause, `${cause.message}, after the host source refused it: ${refused.failure}`),
                        }),
                    ),
                };
            });
            const pieces = yield* Stream.fromIterable(Array.isReadonlyArrayNonEmpty(ready) ? Array.groupWith(ready, (left, right) => Option.isSome(left.facet) === Option.isSome(right.facet)) : []).pipe(
                Stream.flatMap((group) => (Option.isNone(Array.headNonEmpty(group).facet) ? Stream.fromEffect(sent(group)) : Stream.mapEffect(Stream.fromIterable(group), operate))),
                Stream.takeUntil(({ outcomes }) => outcomes.some(Result.isFailure)),
                Stream.runCollect,
            );
            const acted = Array.last(Array.getSomes(Array.map(pieces, Struct.get('acted'))));
            const state = Option.match(acted, { onNone: () => live.state, onSome: Struct.get('state') });
            const outcomes = Array.flatMap(pieces, Struct.get('outcomes'));
            const copied = Array.last(Array.getSomes(pieces.map(({ acted: held }) => Option.flatMapNullishOr(held, Struct.get('pasteboard')))));
            const clipboard = yield* Effect.transposeOption(Option.map(copied, (read) => read.pipe(Effect.fromResult, Effect.flatMap(clipped(live.instance, false)), Effect.result)));
            const touched = new Set([...ready.flatMap(({ step }) => Option.toArray(aimOf(step))), ...outcomes.flatMap((outcome) => (Result.isSuccess(outcome) ? Array.fromNullishOr(outcome.success.resolved?.ref) : []))]);
            const changed = yield* Effect.result(Effect.map(Effect.when(modelled(aimed), Effect.succeed(outcomes.some(Result.isSuccess) && responding(state))), Option.match({ onNone: Array.empty, onSome: (read) => Outline.changes(base, read.base, touched) })));
            yield* Effect.all(
                [
                    maps.prove(
                        live,
                        Array.getSomes(
                            Array.map(
                                Array.flatMap(pieces, ({ entries, outcomes: proven }) => Array.zip(entries, proven)),
                                ([{ at, start }, outcome]) => Option.all({ at, from: Option.some(start), outcome: Option.some(outcome) }),
                            ),
                        ),
                    ),
                    SubscriptionRef.update(
                        running,
                        Array.map((each) => (each.instance === live.instance ? InstanceSnapshot.make({ ...each, state }) : each)),
                    ),
                ],
                { discard: true },
            );
            const frames = [
                ...Option.toArray(Option.flatMap(recording, Result.getFailure)).map((line) => ({ line, image: Option.none() })),
                ...Option.match(
                    Option.flatMap(acted, (held) => Option.fromUndefinedOr(held.recording)),
                    {
                        onNone: Array.empty,
                        onSome: Result.match({ onFailure: ({ message }) => [{ line: `Recording failed: ${message}`, image: Option.none() }], onSuccess: (keyed) => keyFramesOf(Option.none(), live.instance, windowsOf(state), keyed) }),
                    },
                ),
            ];
            yield* Array.isReadonlyArrayNonEmpty(frames) ? attach(frames) : Effect.void;
            yield* stated(row.name, state, {
                troubled: outcomes.some(Result.match({ onFailure: Function.constTrue, onSuccess: ({ proof }) => proof === 'unanswered' })),
                keyed: Array.take(ready, outcomes.length).some(({ step }) => 'key' in step || ('to' in step && Predicate.isString(step.to))),
            });
            const proving = Array.zip(ready, outcomes).flatMap(provingOf(live.instance));
            const handle = yield* Handle;
            yield* handle.set(Proving, { ...(yield* handle.get(Proving)), ...Record.fromEntries(proving) });
            yield* ToolCall.use(Struct.get('awaitRunning'));
            return [
                ...Array.zip(ready, outcomes).map(([entry, outcome], index) => [...(ready.length > 1 ? [`${index + 1}.`] : []), describeOutcome(entry.label, base)(outcome)].join(' ')),
                ...Array.match(Array.drop(ready, outcomes.length), { onEmpty: Array.empty, onNonEmpty: (rest) => [`Not run: ${Outline.list(rest.map(({ label }) => `${VERBS[label.verb][2]} ${label.object}`))}`] }),
                ...Result.match(changed, { onFailure: ({ message }) => [`The changes could not be read: ${message}`], onSuccess: Function.identity }),
                ...Option.match(acted, { onNone: Array.empty, onSome: (held) => diagnose(row.name, state, held) }),
                ...Option.toArray(Option.flatMap(acted, (held) => Option.fromUndefinedOr(held.written))).flatMap(
                    Result.match({ onFailure: ({ message }) => [`The written documents could not be read: ${message}`], onSuccess: Array.match({ onEmpty: Array.empty, onNonEmpty: (urls) => [`Wrote ${Outline.list(urls.map((url) => fileURLToPath(url)))}`] }) }),
                ),
                ...Option.toArray(clipboard).map(Result.match({ onFailure: ({ message }) => `The clipboard could not be read: ${message}`, onSuccess: Function.identity })),
                ...frames.map(Struct.get('line')),
            ].join('\n');
        }),
    );
    yield* Server.registerTool(describeTool(TOOLS.execute_script, descriptions.execute_script).annotateMerge(ends), ({ code }) => Effect.flatMap(current, (aim) => run(aim, code)));
    yield* Server.registerTool(
        describeTool(TOOLS.diagnostics, descriptions.diagnostics),
        Effect.fnUntraced(function* ({ application, ...query }) {
            const read = yield* UndefinedOr.match(application, {
                onUndefined: () => Effect.succeed({ folders: [{ url: pathToFileURL(`${output.logs}/`), glob: '*.log' }] }),
                onDefined: Effect.fnUntraced(function* (given: string) {
                    const [row, runs] = yield* Effect.all([resolve(given), workspace.instancesOf]);
                    const folders = yield* UndefinedOr.match(logs, { onUndefined: () => Effect.succeed([]), onDefined: (kept) => kept(row) });
                    return { application: { url: row.url, ...Option.getOrUndefined(Option.map(Array.head(runs(row).meant), Struct.pick(['instance']))) }, folders };
                }),
            });
            const found = yield* server.diagnostics({ ...query, ...read });
            const [, filed] = Array.mapAccum(
                coalesce(
                    found.logs.map(({ file, text }) =>
                        Option.match(Schema.decodeOption(LogRecord)(text), {
                            onNone: () => logged(Option.some(file), Option.none(), text),
                            onSome: ({ timestamp, attributes, ...record }) => logged(Option.some(file), Option.some(timestamp), printEntry({ ...attributes, ...record }, ['severity_text', 'scope', 'body'])),
                        }),
                    ),
                ),
                Option.none<string>(),
                (last, { file, line }) => [file, [...(Equal.equals(last, file) ? [] : Option.match(file, { onNone: Array.empty, onSome: (held) => [`log ${held}`] })), `  ${line}`]],
            );
            const lines = [
                ...found.reports.map((report) => printEntry(report, ['kind', 'date'])),
                ...found.samples.map((report) => `sample ${report.href}`),
                ...filed.flat(),
                ...Result.match(found.unified, {
                    onFailure: ({ message }) => [`unified log unread: ${message}`],
                    onSuccess: Array.match({
                        onEmpty: Array.empty,
                        onNonEmpty: (events) => [
                            `unified log of ${Array.dedupe(events.map(({ processImagePath }) => basename(processImagePath))).join(', ')}`,
                            ...coalesce(
                                events.map(({ timestamp, messageType, subsystem, category, processImagePath, senderImagePath, eventMessage }) =>
                                    logged(Option.none(), Option.some(timestamp), [messageType, subsystem, category, ...(senderImagePath === processImagePath ? [] : [`from ${basename(senderImagePath)}`]), eventMessage].filter(String.isNonEmpty).join(' ').replaceAll('\n', '\n    ')),
                                ),
                            ).map(({ line }) => `  ${line}`),
                        ],
                    }),
                }),
                ...found.unreadable.map((cause) => `unreadable: ${cause}`),
            ];
            return Array.match(lines, { onEmpty: () => 'no report, sample, log line, or unified log event matched', onNonEmpty: (held) => held.join('\n') });
        }),
    );
});
const Tools = { instructions: INSTRUCTIONS, settings: [Side] as const, register };

// --- [EXPORTS] -------------------------------------------------------------------------

export { Tools };
