import { pathToFileURL } from 'node:url';
import { ApplicationServer } from '@rasm/applications/client';
import { ApplicationFailure, Container, type InstanceSnapshot, InstanceState, QuitMode } from '@rasm/applications/contract';
import { InterfaceMaps } from '@rasm/applications/interface-map';
import { byBundle, byInstance, type Entry, instancesOf, runningOf, Workspace } from '@rasm/applications/workspace';
import { Call, Format, Line, Report } from '@rasm/mcp/report';
import { Handle, Server } from '@rasm/mcp/server';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, ByteSize, Cause, Context, DateTime, Duration, Effect, Formatter, HashSet, Option, Predicate, Record, Result, Schema, Stream, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import { Tool } from 'effect/ai';
import { Reply } from '#/execute-script';
import { Address, Indexed, read } from '#/handle/address';
import { Outline } from '#/handle/outline';
import { find, Hold, hold, openCall, resolve, setTarget, Target } from '#/handle/target';

// --- [MODELS] --------------------------------------------------------------------------

const tool = Tool.make('applications', {
    description: 'Lists the applications that run and the count of those installed, or one application in full with its interface map and running instances. open launches an application behind, or reopens its running instance, and makes it the target of later calls; close quits it, or every application you opened',
    parameters: Schema.Struct({
        action: Schema.optionalKey(Schema.Literals(['list', 'open', 'close']).annotate({ description: 'list when absent' })),
        application: Schema.optionalKey(Schema.String.annotate({ description: 'Listed name, bundle identifier, or bundle path' })),
        installed: Schema.optionalKey(Schema.Boolean.annotate({ description: 'List every installed application by name' })),
        documents: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'File paths or URLs open hands the application, the system default for them when none is named; list shows every application opening them' })),
        arguments: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Process arguments of a new instance' })),
        environment: Schema.optionalKey(Schema.Record(Schema.String, Schema.String).annotate({ description: 'Environment variables of a new instance' })),
        new: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Open an instance beside the running ones' })),
        activate: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Bring the application in front and leave it there' })),
        opening: Schema.optionalKey(Schema.Record(Schema.String, Schema.Union([Schema.Boolean, Schema.Finite, Schema.String])).annotate({ description: 'Open command parameters by the names the listing of the application states' })),
        mode: Schema.optionalKey(QuitMode.annotate({ description: 'What close does with unsaved documents, forcing ending the process with no quit event' })),
        wait: Schema.optionalKey(Schema.Finite.check(Schema.isGreaterThan(0)).annotate({ description: 'Seconds open follows the launch, or close waits at most' })),
    }),
    success: Report.schema,
    failure: Schema.ErrorInstance(),
    dependencies: [Handle],
})
    .annotateMerge(Context.make(Tool.Destructive, false).pipe(Context.add(Tool.Idempotent, true), Context.add(Tool.OpenWorld, false), Context.add(Tool.Strict, true), Context.add(Tool.Meta, { 'anthropic/alwaysLoad': true })))
    .annotate(Server.Instructions, ['Start with applications: it lists what runs, and open makes one application the target of the calls that follow.', 'Results lead with what the target did since your last call, and name the call that answers a dialog or continues a cut.']);

// --- [SERVICES] ------------------------------------------------------------------------

const Intent = Context.Reference<number>('@rasm/application-tools/applications/Intent', { defaultValue: () => 0 });
const Bundles = Context.Reference<readonly Entry[]>('@rasm/application-tools/applications/Bundles', { defaultValue: Array.empty });

// --- [OPERATIONS] ----------------------------------------------------------------------

const same = <A extends Pick<Entry, 'url'>>(left: A, right: A) => left.url.href === right.url.href;
const locate = (document: string) => (URL.canParse(document) ? new URL(document) : pathToFileURL(document));
const asking = (state: InstanceSnapshot['state']) => InstanceState.matchOrElse(state, { running: ({ windows }) => windows.filter(Struct.get('asking')).map(({ kind = 'window', container, title }) => `asking ${Outline.caption(kind, container, title).join(' ')}`) }, Array.empty<string>);
const placed = (row: Entry, held: Context.Service.Shape<typeof Target>['held']) => (each: InstanceSnapshot) => [
    ...InstanceState.match(each.state, {
        running: ({ status, active, windows, statusItems = 0, statusItemsFailure }) => [
            ...InstanceState.Status.matchOrElse(status, { responding: () => [], unresponsive: ({ _tag, thread }) => [_tag, `main thread ${thread}`] }, ({ _tag }) => [_tag]),
            ...(active ? [Outline.say.front] : []),
            ...[[Array.countBy(windows, (window) => window.presented && window.kind !== 'menu'), Outline.nouns.window] as const, [statusItems, Outline.nouns.statusItem] as const].flatMap(([amount, noun]) => (amount > 0 ? [Format.count(amount, noun)] : [])),
            ...Array.fromNullishOr(statusItemsFailure).map((message) => `status items unread: ${message}`),
        ],
        failed: ({ reason, subject, message }) => [`state unread: ${message ?? `${subject} ${reason}`}`],
        exited: ({ _tag }) => [_tag],
    }),
    ...(each.url.href === row.url.href ? [] : [`from ${Format.path(each.url)}`]),
    ...(Option.exists(held, (aim) => aim.instance === each.instance) ? ['target'] : []),
];
const listing = Effect.fnUntraced(function* ({ documents = [], installed = false }: Pick<Tool.Parameters<typeof tool>, 'documents' | 'installed'>) {
    const [{ catalog, running }, server, handle] = yield* Effect.all([Workspace, ApplicationServer, Handle]);
    const [entries, instances, { held, followed }, handlers] = yield* Effect.all([SubscriptionRef.get(catalog), SubscriptionRef.get(running), handle.get(Target), Effect.forEach(documents, (document) => server.handlers({ document: locate(document) }), { concurrency: 'unbounded' })]);
    const [bundles, runs] = [byBundle(entries), instancesOf(instances, followed)];
    const repeated = Record.filter(Array.groupBy(entries, Struct.get('name')), (alike) => alike.length > 1);
    const named = (row: Entry) => Format.quoted(Record.has(repeated, row.name) ? `${row.name} at ${Format.path(row.url)}` : row.name);
    const opening = Array.zip(handlers, documents).flatMap(([{ installations }, document]) => installations.map(({ url }) => ({ bundle: url.href, document: Format.quoted(document) })));
    const scope = Array.match(documents, {
        onEmpty: () => entries.map((row) => ({ row, opens: Array.empty<string>() })),
        onNonEmpty: () => Record.values(Record.intersection(Array.groupBy(opening, Struct.get('bundle')), bundles, (found, row) => ({ row, opens: found.map(Struct.get('document')) }))),
    });
    const [own, driven] = Array.partition(
        scope.map((each) => ({ ...each, meant: runs(each.row).meant })).filter(({ opens, meant }) => Array.isReadonlyArrayNonEmpty(opens) || Array.isReadonlyArrayNonEmpty(meant)),
        (each) => Option.match(each.row.claim, { onNone: () => Result.succeed(each), onSome: ({ server: owner }) => Result.fail({ owner, row: each.row }) }),
    );
    const shown = new Set([...own, ...driven].map(({ row }) => row.url.href));
    const top = entries.filter(({ url }) => !Array.unfold(new URL('..', url), (at) => (at.pathname === '/' ? Option.none() : Option.some([at.href, new URL('..', at)] as const))).some((href) => Record.has(bundles, href)));
    const rest = installed ? new Line({ head: ['installed'], value: Format.list(top.filter(({ url }) => !shown.has(url.href)).map(named)) }) : new Line({ head: ['installed'], value: Format.count(top.length), call: new Call({ tool: tool.name, given: { installed: true } }) });
    yield* Effect.when(new Cause.NoSuchElementError(`${Format.list(documents.map(Format.quoted))} opened by no listed application`), Effect.succeed(Array.isReadonlyArrayNonEmpty(documents) && shown.size === 0));
    return new Report({
        lines: [
            ...own.map(({ row, opens, meant }) => new Line({ head: [named(row)], facts: [...Array.take(meant, 1).flatMap(placed(row, held)), ...(Array.isReadonlyArrayEmpty(opens) ? [] : [`opens ${Format.list(opens)}`])] })),
            ...Record.collect(Array.groupBy(driven, Struct.get('owner')), (owner, claimed) => new Line({ head: ['driven by', owner], value: Format.list(claimed.map(({ row }) => named(row))) })),
            ...(Array.isReadonlyArrayEmpty(documents) ? [rest] : []),
        ],
    });
});
const listed = Effect.fnUntraced(function* (given: string) {
    const [{ running }, server, maps, handle, row] = yield* Effect.all([Workspace, ApplicationServer, InterfaceMaps, Handle, find(given)]);
    const [installation, dictionary, languages, instances, { held, followed }, now] = yield* Effect.all([
        server.installation({ url: row.url }),
        Effect.option(Effect.tapError(server.dictionary({ url: row.url }), (cause) => Effect.logDebug('Scripting dictionary unread', cause))),
        Effect.option(Effect.tapError(maps.held(row.identifier), (cause) => Effect.logWarning('Interface map unread', cause))),
        SubscriptionRef.get(running),
        handle.get(Target),
        DateTime.now,
    ]);
    const mapped = Option.getOrElse(languages, Record.empty);
    const stated = Record.map<string, Option.Option<unknown>, unknown>(
        {
            ...Struct.pick(installation, ['scriptable', 'multipleInstancesProhibited', 'urlSchemes']),
            documents: Option.map(installation.documentTypes, (types) => types.flatMap(Struct.get('extensions'))),
            openTakes: Option.map(Option.flatMap(dictionary, Struct.get('open')), ({ parameters }) => parameters.map(Struct.get('name'))),
            quarantine: Option.map(installation.quarantine, Struct.pick(['agentName', 'originURL'])),
            appIntents: Option.map(installation.appIntents, (intents) =>
                intents.map(({ identifier, title = identifier, openAppWhenRun, parameters }) => ({ intent: title, opensApplication: openAppWhenRun, takes: parameters.map(Struct.get('name')), optional: parameters.filter(Struct.get('isOptional')).map(Struct.get('name')) })),
            ),
        },
        Option.getOrUndefined,
    );
    const usage = ({ usage: used, usageFailure, launchDate }: InstanceSnapshot) => [
        ...Array.fromNullishOr(used).flatMap(({ footprint, cpu }) => [`memory ${Format.bytes(ByteSize.bytes(footprint))}`, `processor time ${Format.duration(Duration.seconds(cpu))}`]),
        ...Array.fromNullishOr(usageFailure).map((message) => `usage unread: ${message}`),
        ...Array.fromNullishOr(launchDate).map((launched) => `up ${Format.duration(DateTime.distance(launched, now))}`),
    ];
    return new Report({
        lines: [
            new Line({ head: [row.name], facts: [...Record.toEntries(row.facts).flatMap(([key, value]) => (value === row.identifier ? [] : [`${key} ${Predicate.isString(value) ? value : Formatter.format(value)}`])), ...Option.toArray(row.claim).map(({ server: owner }) => `driven by ${owner}`)] }),
            ...Report.of({ path: Format.path(installation.url), ...(installation.activationPolicy === 'regular' ? {} : Struct.pick(installation, ['activationPolicy'])), ...stated, unread: installation.unread }).lines,
            ...Record.collect(mapped, (language, { coverage: { known, earlier, openers } }) => {
                const kinds = Record.toEntries(known).flatMap(([kind, amount]) => (Schema.is(Container.fields.kind)(kind) ? [Format.count(amount, Outline.nouns[kind])] : []));
                const gaps = [[earlier, 'of earlier builds'] as const, [openers.length, 'openers never seen open'] as const].flatMap(([amount, noun]) => (amount > 0 ? [`${Format.count(amount)} ${noun}`] : []));
                return new Line({ head: ['map', ...(Record.size(mapped) > 1 ? [language] : [])], value: Format.list(kinds), facts: gaps });
            }),
            ...instancesOf(instances, followed)(row).meant.map((each) => new Line({ head: ['running'], facts: [...placed(row, held)(each), ...usage(each)] })),
        ],
    });
});
const opened = Effect.fnUntraced(function* (application: string | undefined, { documents = [], wait, activate = false, ...launch }: Pick<Tool.Parameters<typeof tool>, 'documents' | 'wait' | 'activate' | 'arguments' | 'environment' | 'new' | 'opening'>) {
    const [server, handle, call] = yield* Effect.all([ApplicationServer, Handle, Effect.fiberId]);
    const handler = Effect.gen(function* () {
        const opening = yield* Effect.forEach(documents, (document) => Effect.map(server.handlers({ document: locate(document) }), Struct.get('installations')), { concurrency: 'unbounded' });
        const defaults = Array.dedupeWith(Array.getSomes(opening.map(Array.head)), same);
        const every = Array.reduce(Array.drop(opening, 1), Array.flatten(Array.take(opening, 1)), Array.intersectionWith(same));
        return Predicate.isTupleOf(defaults, 1) ? defaults[0].url.href : yield* new Cause.IllegalArgumentError(Report.text(Report.of({ application: 'required', defaults: defaults.map(Struct.get('name')), opensEveryDocument: every.map(Struct.get('name')) })));
    });
    const row = yield* Effect.flatMap(UndefinedOr.match(application, { onUndefined: () => handler, onDefined: Effect.succeed }), resolve);
    const stated = application === undefined || Array.containsWith(Unmatched.named)([row.name, row.product, row.identifier, ...row.aliases], application);
    const [known] = yield* Effect.all([handle.get(Indexed), handle.set(Intent, call), handle.update(Bundles, (held) => (stated ? held : Array.dedupeWith([...held, row], same)))]);
    const claim = (instance: InstanceSnapshot['instance']) => Effect.flatMap(handle.get(Intent), (latest) => Effect.when(setTarget({ row, instance }), Effect.succeed(latest === call)));
    const launched = yield* server.runLaunch({ url: row.url, documents: documents.map(locate), ...launch }, Option.map(Option.fromUndefinedOr(wait), Duration.seconds), ({ instance }) =>
        Effect.andThen(hold([Hold.Launching({ instance, name: row.name })]), Effect.ignore(claim(instance), { log: 'Warn', message: 'Target not taken as the instance launched' })),
    );
    const verb = launched.created ? 'launched' : 'reopened';
    const front = yield* Effect.when(Effect.result(server.activate({ instance: launched.instance })), Effect.succeed(activate));
    const live = yield* Effect.fromOption(yield* claim(launched.instance), () => new Cause.NoSuchElementError(Report.text(new Report({ lines: [new Line({ head: [row.name, verb], facts: ['target taken by a later open'], call: openCall(row.name) })] }))));
    const [aims, answered] = yield* Effect.all([Server.use(({ states }) => Stream.runHead(states(Target))), Effect.transposeOption(Option.map(launched.opened, (value) => Effect.orDie(Schema.encodeEffect(Reply)(value))))]);
    const { active, hidden } = InstanceState.matchOrElse(live.state, { running: (state) => ({ active: state.active, hidden: state.windows.some(({ kind }) => kind !== 'menu') }) }, () => ({ active: false, hidden: false }));
    const before = Option.filter(known, (held) => held.instance === live.instance);
    const outline = yield* read({ row, live }).pipe(
        Effect.flatMap(({ base, indexed }) => {
            const shown = base.shown.map(Struct.get('container')).filter(({ kind }) => kind !== 'menuBar' && kind !== 'menu');
            const changed = Option.map(before, ({ index }) => Outline.changes(index, base, new Set()));
            const fresh = new Set(Option.getOrElse(changed, Array.empty<Line>).flatMap(({ head }) => Array.take(head, 1)));
            const kept = shown.filter(({ kind, ref }) => !fresh.has(Address.token(kind, ref))).map(({ kind, ref, title }) => new Line({ head: Outline.caption(kind, ref, title) }));
            const bare = new Line({ head: ['windows'], value: 'none shown', ...(hidden ? { call: new Call({ tool: 'view', given: { of: 'all' } }) } : {}) });
            return Array.isReadonlyArrayEmpty(shown) ? Effect.succeed([bare]) : Option.match(changed, { onNone: () => Effect.fromResult(Outline.render(indexed(shown))), onSome: (lines) => Effect.succeed([...kept, ...lines]) });
        }),
        Effect.catch(({ message }) => Effect.succeed([new Line({ head: ['interface'], value: 'unread', facts: [message] })])),
    );
    const subject = yield* Outline.subject(row.name, live, {
        verb: active ? verb : `${verb} behind`,
        facts: [
            ...Option.toArray(front).flatMap(Result.match({ onFailure: ({ message }) => [`bring to front failed: ${message}`], onSuccess: ({ _tag }) => (_tag === 'forward' ? [] : ['bring to front refused']) })),
            ...(live.url.href === row.url.href ? [] : [`from ${Format.path(live.url)}`]),
            ...(launched.replaced === true ? ['code changed on disk since launch'] : []),
            ...(Array.countBy(Option.getOrElse(aims, Array.empty<Context.Service.Shape<typeof Target>>), ({ held }) => Option.exists(held, (aim) => aim.instance === live.instance)) > 1 ? ['also driven by another agent'] : []),
        ],
    });
    const failures = launched.failed.map(({ subject: failing, reason, message }) => new Line({ head: [failing, reason], ...(message === undefined ? {} : { value: message }) }));
    return new Report({ lines: [...subject, ...failures, ...Report.of({ openCommand: Option.getOrUndefined(answered) }).lines, ...outline] });
}, Effect.scoped);
const closed = Effect.fnUntraced(function* (application: string | undefined, { wait, mode }: Pick<Tool.Parameters<typeof tool>, 'wait' | 'mode'>) {
    const [{ running }, server, handle] = yield* Effect.all([Workspace, ApplicationServer, Handle]);
    const [instances, { followed, opened: own }, chosen] = yield* Effect.all([SubscriptionRef.get(running), handle.get(Target), Effect.transposeOption(Option.map(Option.fromUndefinedOr(application), resolve))]);
    const found = Option.map(chosen, (row) => ({ row, ...instancesOf(instances, followed)(row) }));
    const quit = Option.match(found, { onNone: () => Record.values(runningOf(instances)).filter(({ instance }) => HashSet.has(own, instance)), onSome: Struct.get('meant') });
    const refusal = Option.match(found, {
        onNone: () => new Line({ head: ['application'], value: 'required', facts: ['nothing you opened runs'] }),
        onSome: ({ row, apart }) =>
            Option.match(Array.head(apart), {
                onNone: () => new Line({ head: [row.name, 'not running'] }),
                onSome: ({ url }) => new Line({ head: [row.name, 'runs from', Format.path(url)], facts: ['not opened by you'], call: new Call({ tool: tool.name, given: { action: 'close', application: Format.path(url) } }) }),
            }),
    });
    yield* Effect.when(new Cause.NoSuchElementError(Report.text(new Report({ lines: [refusal] }))), Effect.succeed(Array.isReadonlyArrayEmpty(quit)));
    const bound = UndefinedOr.match(wait, { onUndefined: () => Duration.infinity, onDefined: Duration.seconds });
    yield* hold(quit.map(({ instance }) => Hold.Quitting({ instance })));
    const outcomes = yield* Effect.forEach(quit, ({ instance }) => Effect.result(Effect.timeoutOption(server.quit({ instance, ...(mode === undefined ? {} : { mode }) }), bound)), { concurrency: 'unbounded' });
    const after = byInstance(yield* SubscriptionRef.get(running));
    const standing = (instance: InstanceSnapshot['instance']) => Option.toArray(Record.get(after, instance)).flatMap(({ state }) => asking(state));
    const verdicts = Array.zipWith(quit, outcomes, ({ instance }, outcome) =>
        Result.match(outcome, {
            onFailure: (error) => ({ words: Schema.is(ApplicationFailure)(error) ? 'did not quit' : 'quit unconfirmed', value: error.message, facts: standing(instance) }),
            onSuccess: Option.match({
                onNone: () => ({ words: 'still runs', facts: [`after ${Format.duration(bound)}`, ...standing(instance)] }),
                onSome: InstanceState.match({
                    exited: () => ({ words: 'quit', facts: Array.empty<string>() }),
                    running: (state) => ({ words: 'still runs', facts: asking(state) }),
                    failed: ({ reason, subject, message }) => ({ words: 'state unread', value: message ?? `${subject} ${reason}`, facts: Array.empty<string>() }),
                }),
            }),
        }),
    );
    return new Report({ lines: Array.zipWith(quit, verdicts, (each, { words, ...verdict }) => new Line({ ...verdict, head: [each.name ?? each.identifier, ...(Option.exists(chosen, ({ url }) => url.href !== each.url.href) ? ['from', Format.path(each.url)] : []), words] })) });
}, Effect.scoped);

// --- [COMPOSITION] ---------------------------------------------------------------------

const Applications = {
    tool,
    handle: Effect.fnUntraced(function* ({ action = 'list', application, ...given }: Tool.Parameters<typeof tool>) {
        const bundles = yield* Handle.use((handle) => handle.get(Bundles));
        const named = UndefinedOr.map(application, (name) => Option.getOrElse(Array.findFirst(bundles, (row) => Array.containsWith(Unmatched.named)([row.name, row.identifier, ...row.aliases], name)).pipe(Option.map(({ url }) => url.href)), () => name));
        return yield* { open: () => opened(named, Struct.omit(given, ['installed', 'mode'])), close: () => closed(named, given), list: () => UndefinedOr.match(named, { onDefined: listed, onUndefined: () => listing(given) }) }[action]();
    }),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Applications };
