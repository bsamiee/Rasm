import { pathToFileURL } from 'node:url';
import { Catalog, Entry } from '@rasm/applications/catalog';
import { InterfaceMap } from '@rasm/applications/map';
import { ApplicationServer } from '@rasm/applications/server/client';
import { Element } from '@rasm/applications/server/element';
import { ApplicationFailure } from '@rasm/applications/server/failure';
import type { Installation } from '@rasm/applications/server/installation';
import { Instance } from '@rasm/applications/server/instance';
import { Handle, Server } from '@rasm/mcp/server';
import { ToolCall } from '@rasm/mcp/toolkit';
import { Call, Format, Line, Report } from '@rasm/report';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, ByteSize, Cause, Context, DateTime, Duration, Effect, Equivalence, Formatter, HashSet, Option, Predicate, Record, Result, Schema, Stream, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import { Tool } from 'effect/ai';
import { Act } from '#/act';
import { Address } from '#/handle/address';
import { Outline } from '#/handle/outline';
import { Target } from '#/handle/target';

// --- [MODELS] --------------------------------------------------------------------------

const tool = Tool.make('applications', {
    description: 'Lists running applications and the installed count, or one application in full with its interface map and instances. open launches an application behind or reopens its running instance, makes it the target of later calls, and returns what it shows; close quits it, or every application you opened',
    parameters: Schema.Struct({
        action: Schema.optionalKey(Schema.Literals(['list', 'open', 'close']).annotate({ description: 'list when absent' })),
        application: Schema.optionalKey(Schema.String.annotate({ description: 'Listed name, bundle identifier, or bundle path' })),
        installed: Schema.optionalKey(Schema.Boolean.annotate({ description: 'List every installed application by name' })),
        documents: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'File paths or URLs open passes to the application, or to their default application when none is named; list shows each application that opens them' })),
        arguments: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Process arguments of a new instance' })),
        environment: Schema.optionalKey(Schema.Record(Schema.String, Schema.String).annotate({ description: 'Environment variables of a new instance' })),
        new: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Open an instance beside running ones' })),
        activate: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Bring the application in front and leave it there' })),
        opening: Schema.optionalKey(Schema.Record(Schema.String, Schema.Union([Schema.Boolean, Schema.Finite, Schema.String])).annotate({ description: 'Open command parameters, named as open takes lists them for the application' })),
        mode: Schema.optionalKey(Instance.QuitMode.annotate({ description: 'What close does with unsaved documents, asking when absent; forcing ends the process with no quit event' })),
        wait: Schema.optionalKey(Schema.Finite.check(Schema.isGreaterThan(0)).annotate({ description: 'Seconds close waits for the quit at most' })),
    }),
    success: Report.schema,
    failure: Schema.ErrorInstance(),
    dependencies: [Handle],
})
    .annotateMerge(Context.make(Tool.Destructive, false).pipe(Context.add(Tool.Idempotent, true), Context.add(Tool.OpenWorld, false), Context.add(Tool.Meta, { 'anthropic/alwaysLoad': true })))
    .annotate(Server.Instructions, ['Start with applications: it lists what runs, and open makes one application the target of the calls that follow.', 'Results lead with what the target did since your last call, and name the call that answers a dialog or continues a cut.']);

// --- [SERVICES] ------------------------------------------------------------------------

const Opens = Context.Reference<{ readonly latest: number; readonly bundles: Record.ReadonlyRecord<string, Entry> }>('@rasm/application-tools/applications/Opens', { defaultValue: () => ({ latest: 0, bundles: {} }) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const parse = (document: string) => (URL.canParse(document) ? new URL(document) : pathToFileURL(document));
const unread = (reading: string, { message }: Pick<Error, 'message'>) => `${reading} unread: ${message}`;
const print = (row: Pick<Entry, 'url'>, held: (typeof Target)['Service']['held'], { instance, url, state }: Pick<Instance, 'instance' | 'url' | 'state'>) => [
    ...Instance.State.match(state, {
        running: ({ status, active, windows, statusItems = Result.succeed(0) }) => [
            ...Instance.State.Status.matchOrElse(status, { responding: Array.empty<string>, unresponsive: ({ _tag, thread }) => [_tag, `main thread ${thread}`] }, ({ _tag }) => [_tag]),
            ...(active ? [Outline.say.front] : []),
            ...[[Array.countBy(windows, (window) => window.presented && window.kind !== 'menu'), Outline.nouns.window] as const, [Result.getOrElse(statusItems, () => 0), Outline.nouns.statusItem] as const].flatMap(([amount, noun]) => (amount > 0 ? [Format.count(amount, noun)] : [])),
            ...statusItems.pipe(Result.getFailure, Option.toArray).map((failure) => unread(Outline.nouns.statusItem.other, failure)),
            ...windows.filter(Struct.get('asking')).map(({ kind = 'window', container, title }) => `asking ${Outline.caption(kind, container, title).join(' ')}`),
        ],
        failed: ({ reason, subject, values }) => [unread('state', ApplicationFailure.of(reason, subject, values))],
        exited: Array.empty<string>,
    }),
    ...(url.href === row.url.href ? [] : [`from ${Format.path(url)}`]),
    ...(Option.exists(held, (aim) => aim.instance === instance) ? ['target'] : []),
];
const list = Effect.fnUntraced(function* ({ documents = [], installed = false }: Pick<Tool.Parameters<typeof tool>, 'documents' | 'installed'>) {
    const [{ entries: catalog }, server, handle] = yield* Effect.all([Catalog, ApplicationServer, Handle]);
    const [entries, instances, { held, followed }, listings] = yield* Effect.all([SubscriptionRef.get(catalog), SubscriptionRef.get(server.running), handle.get(Target), Effect.forEach(documents, (document) => server.installationsOpening({ document: parse(document) }), { concurrency: 'unbounded' })]);
    const [bundles, runs] = [Entry.byBundle(entries), Entry.instancesOf(instances, followed)];
    const repeated = Record.filter(Array.groupBy(entries, Struct.get('name')), (alike) => alike.length > 1);
    const name = (row: Entry) => Format.quoted(Record.has(repeated, row.name) ? `${row.name} at ${Format.path(row.url)}` : row.name);
    const openers = Array.zip(listings, documents).flatMap(([{ installations }, document]) => installations.map(({ url }) => ({ bundle: url.href, document: Format.quoted(document) })));
    const scope = Array.match(documents, {
        onEmpty: () => entries.map((row) => ({ row, opens: Array.empty<string>() })),
        onNonEmpty: () => Record.values(Record.intersection(Array.groupBy(openers, Struct.get('bundle')), bundles, (found, row) => ({ row, opens: [`opens ${Format.list(found.map(Struct.get('document')))}`] }))),
    });
    const [own, driven] = Array.partition(
        scope.map((each) => ({ ...each, meant: runs(each.row).meant })).filter(({ opens, meant }) => Array.isReadonlyArrayNonEmpty(opens) || Array.isReadonlyArrayNonEmpty(meant)),
        (each) => Option.match(each.row.claim, { onNone: () => Result.succeed(each), onSome: ({ server: owner }) => Result.fail({ owner, row: each.row }) }),
    );
    const shown = new Set([...own, ...driven].map(({ row }) => row.url.href));
    const top = entries.filter(({ url }) => !Array.unfold(new URL('..', url), (at) => (at.pathname === '/' ? Option.none() : Option.some([at.href, new URL('..', at)] as const))).some((href) => Record.has(bundles, href)));
    const rest = installed ? new Line({ head: ['installed'], value: Format.list(top.filter(({ url }) => !shown.has(url.href)).map(name)) }) : new Line({ head: ['installed'], value: Format.count(top.length), call: new Call({ tool: tool.name, given: { installed: true } }) });
    yield* Effect.when(new Cause.NoSuchElementError(Report.text(new Report({ lines: [new Line({ head: ['documents'], value: Format.list(documents.map(Format.quoted)), facts: ['no listed application opens them'] })] }))), Effect.succeed(Array.isReadonlyArrayNonEmpty(documents) && shown.size === 0));
    return new Report({
        lines: [
            ...own.map(({ row, opens, meant }) => new Line({ head: [name(row)], facts: [...Array.take(meant, 1).flatMap((each) => print(row, held, each)), ...opens] })),
            ...Record.collect(Array.groupBy(driven, Struct.get('owner')), (owner, claimed) => new Line({ head: ['driven by', owner], value: Format.list(claimed.map(({ row }) => name(row))) })),
            ...(Array.isReadonlyArrayEmpty(documents) ? [rest] : []),
        ],
    });
});
const describe = Effect.fnUntraced(function* (given: string) {
    const [server, maps, handle, row] = yield* Effect.all([ApplicationServer, InterfaceMap.Store, Handle, Target.find(given)]);
    const [installation, dictionary, mapped, instances, { held, followed }, now] = yield* Effect.all([server.installation({ url: row.url }), Effect.result(server.dictionary({ url: row.url })), Effect.result(maps.read(row.identifier)), SubscriptionRef.get(server.running), handle.get(Target), DateTime.now]);
    const facts = Record.map<string, Option.Option<unknown>, unknown>(
        {
            path: Option.some(Format.path(installation.url)),
            activationPolicy: Option.liftPredicate(installation.activationPolicy, (policy) => policy !== 'regular'),
            ...Struct.pick(installation, ['scriptable', 'multipleInstancesProhibited', 'urlSchemes', 'quarantine']),
            documents: Option.map(installation.documentTypes, (types) => types.flatMap(Struct.get('extensions'))),
            openTakes: Option.map(Option.flatMap(Result.getSuccess(dictionary), Struct.get('open')), ({ parameters }) => parameters.map(Struct.get('name'))),
            appIntents: Option.map(installation.appIntents, (intents) =>
                intents.map(({ identifier, title = identifier, openAppWhenRun, parameters }) => ({ intent: title, opensApplication: openAppWhenRun, takes: parameters.map(Struct.get('name')), optional: parameters.filter(Struct.get('isOptional')).map(Struct.get('name')) })),
            ),
        },
        Option.getOrUndefined,
    );
    const failures = { installation: installation.unread ?? [], 'scripting dictionary': Option.toArray(Option.filter(Result.getFailure(dictionary), () => Option.contains(installation.scriptable, true))), map: mapped.pipe(Result.getFailure, Option.toArray) };
    const languages = Result.getOrElse(mapped, Record.empty<string, InterfaceMap>);
    const { meant } = Entry.instancesOf(instances, followed)(row);
    return new Report({
        lines: [
            new Line({ head: [row.name], facts: [...Record.toEntries(row.facts).flatMap(([key, value]) => (value === row.identifier ? [] : [`${key} ${Predicate.isString(value) ? value : Formatter.format(value)}`])), ...Option.toArray(row.claim).map(({ server: owner }) => `driven by ${owner}`)] }),
            ...Report.of(facts).lines,
            ...Record.collect(failures, (reading, failed) => failed.map((failure) => new Line({ head: [unread(reading, failure)] }))).flat(),
            ...Record.collect(languages, (language, { coverage: { containers, earlier, openers } }) => {
                const kinds = Record.toEntries(containers).flatMap(([kind, amount]) => (Schema.is(Element.cases.container.fields.kind)(kind) ? [Format.count(amount, Outline.nouns[kind])] : []));
                const gaps = [[earlier, { other: 'of earlier builds' }] as const, [openers.length, { one: 'opener never seen open', other: 'openers never seen open' }] as const].flatMap(([amount, noun]) => (amount > 0 ? [Format.count(amount, noun)] : []));
                return new Line({ head: ['map', ...(Record.size(languages) > 1 ? [language] : [])], value: Format.list(kinds), facts: gaps });
            }),
            ...meant.map((each) => {
                const usage = Array.fromNullishOr(each.usage).flatMap(Result.match({ onFailure: (failure) => [unread('usage', failure)], onSuccess: ({ footprint, cpu }) => [`memory ${Format.bytes(ByteSize.bytes(footprint))}`, `processor time ${Format.duration(cpu)}`] }));
                return new Line({ head: [each.state._tag], facts: [...print(row, held, each), ...usage, ...Array.fromNullishOr(each.launchDate).map((launched) => `up ${Format.duration(DateTime.distance(launched, now))}`)] });
            }),
        ],
    });
});
const open = Effect.fnUntraced(function* (application: string | undefined, { documents = [], activate = false, ...given }: Pick<Tool.Parameters<typeof tool>, 'documents' | 'activate' | 'arguments' | 'environment' | 'new' | 'opening'>) {
    const [server, catalog, handle, { call }] = yield* Effect.all([ApplicationServer, Catalog, Handle, ToolCall]);
    const urls = documents.map(parse);
    const preferred = Effect.gen(function* () {
        const bundle = Equivalence.mapInput(Equivalence.String, ({ url }: Installation) => url.href);
        const listings = yield* Effect.forEach(urls, (document) => Effect.map(server.installationsOpening({ document }), Struct.get('installations')), { concurrency: 'unbounded' });
        const defaults = Array.dedupeWith(
            listings.flatMap((found) => Array.take(found, 1)),
            bundle,
        );
        const every = Array.reduce(Array.drop(listings, 1), Array.flatten(Array.take(listings, 1)), Array.intersectionWith(bundle));
        return Predicate.isTupleOf(defaults, 1) ? defaults[0].url.href : yield* new Cause.IllegalArgumentError(Report.text(Report.of({ application: 'required', defaults: defaults.map(Struct.get('name')), opensEveryDocument: every.map(Struct.get('name')) })));
    });
    const row = yield* Effect.flatMap(UndefinedOr.match(application, { onUndefined: () => preferred, onDefined: Effect.succeed }), Target.resolve);
    const listed = Entry.byBundle(yield* SubscriptionRef.get(catalog.entries));
    yield* handle.update(Opens, ({ bundles }) => ({ latest: call, bundles: Record.has(listed, row.url.href) ? bundles : Record.set(bundles, row.url.href, row) }));
    const claim = (instance: Instance['instance']) => Effect.flatMap(handle.get(Opens), ({ latest }) => Effect.when(Target.set({ row, instance }), Effect.succeed(latest === call)));
    const launched = yield* server
        .launch({ url: row.url, documents: urls, ...Struct.pick(given, ['arguments', 'environment', 'new', 'opening']) }, ({ instance }) =>
            Effect.andThen(Target.hold([Target.Hold.Launching({ instance, name: row.name })]), Effect.ignore(claim(instance), { log: 'Warn', message: 'Target not taken as the instance launched' })),
        )
        .pipe(Effect.catchIf(Schema.is(ApplicationFailure), (failure) => new Cause.UnknownError(failure, Report.text(new Report({ lines: [new Line({ head: [failure.message] })], ...Option.getOrUndefined(Option.map(Record.get(Act.remedies(row.name), failure.reason), (next) => ({ next }))) })))));
    const verb = launched.created ? launched._tag : 'reopened';
    const front = yield* Effect.when(Effect.result(server.activate({ instance: launched.instance })), Effect.succeed(activate));
    const live = yield* Effect.fromOption(yield* claim(launched.instance), () => new Cause.NoSuchElementError(Report.text(new Report({ lines: [new Line({ head: [row.name, verb], facts: ['target taken by a later open'], call: Target.open(row.name) })] }))));
    const aims = yield* Server.use(({ states }) => Stream.runHead(states(Target)));
    const { active, hidden } = Instance.State.matchOrElse(live.state, { running: (state) => ({ active: state.active, hidden: state.windows.some(({ kind }) => kind !== 'menu') }) }, () => ({ active: false, hidden: false }));
    const before = Option.filter(yield* handle.get(Address.Held), (last) => last.instance === live.instance);
    const outline = yield* Address.read({ row, live }).pipe(
        Effect.flatMap((read) => {
            const shown = read.model.elements.flatMap((element) => (element._tag === 'container' && element.kind !== 'menuBar' && element.kind !== 'menu' ? [element] : []));
            const changed = Option.map(before, (last) => Outline.changes(last, read, new Set()));
            const fresh = new Set(Option.getOrElse(changed, Array.empty<Line>).flatMap(({ head }) => Array.take(head, 1)));
            const kept = shown.filter(({ kind, ref }) => !fresh.has(Address.refer(kind, ref))).map(({ kind, ref, title }) => new Line({ head: Outline.caption(kind, ref, title) }));
            const bare = new Line({ head: ['windows'], value: 'none shown', ...(hidden ? { call: new Call({ tool: 'view', given: { of: 'all' } }) } : {}) });
            return Array.isReadonlyArrayEmpty(shown) ? Effect.succeed([bare]) : Option.match(changed, { onNone: () => Effect.fromResult(Outline.render(Address.narrow(read, shown))), onSome: (lines) => Effect.succeed([...kept, ...lines]) });
        }),
        Effect.catch((error) => Effect.succeed([new Line({ head: [unread('interface', error)] })])),
    );
    const lead = yield* Outline.subject(row.name, live, {
        verb: active ? verb : `${verb} behind`,
        facts: [
            ...Option.toArray(front).flatMap(Result.match({ onFailure: ({ message }) => [message], onSuccess: Instance.Presentation.matchOrElse({ forward: Array.empty<string> }, ({ _tag }) => [`activate ${_tag}`]) })),
            ...(live.url.href === row.url.href ? [] : [`from ${Format.path(live.url)}`]),
            ...(launched.replacedOnDisk === true ? ['replaced on disk since launch'] : []),
            ...(Array.countBy(Option.getOrElse(aims, Array.empty<(typeof Target)['Service']>), ({ held }) => Option.exists(held, (aim) => aim.instance === live.instance)) > 1 ? ['also driven by another agent'] : []),
        ],
    });
    const failures = launched.failed.map(({ subject, reason, values }) => new Line({ head: [ApplicationFailure.of(reason, subject, values).message] }));
    return new Report({ lines: [...lead, ...failures, ...Report.of({ openCommand: Option.getOrUndefined(launched.opened) }).lines, ...outline] });
}, Effect.scoped);
const close = Effect.fnUntraced(function* (application: string | undefined, { wait, ...given }: Pick<Tool.Parameters<typeof tool>, 'wait' | 'mode'>) {
    const [server, handle] = yield* Effect.all([ApplicationServer, Handle]);
    const [instances, { held, followed, opened }, chosen] = yield* Effect.all([SubscriptionRef.get(server.running), handle.get(Target), Effect.transposeOption(Option.map(Option.fromUndefinedOr(application), Target.resolve))]);
    const found = Option.map(chosen, Entry.instancesOf(instances, followed));
    const quit = Option.match(found, { onNone: () => Record.values(Instance.running(instances)).filter(({ instance }) => HashSet.has(opened, instance)), onSome: Struct.get('meant') });
    const elsewhere = Option.flatMap(found, ({ apart }) => Array.head(apart));
    const moved = Option.zipWith(chosen, elsewhere, ({ name }, { url }) => new Line({ head: [name, 'runs from', Format.path(url)], facts: ['not opened by you'], call: new Call({ tool: tool.name, given: { action: 'close', application: Format.path(url) } }) }));
    const stopped = Option.map(chosen, ({ name }) => new Line({ head: [name, 'not running'] }));
    const refusal = Option.getOrElse(Option.firstSomeOf([moved, stopped]), () => new Line({ head: ['application'], value: 'required', facts: ['nothing you opened runs'] }));
    yield* Effect.when(new Cause.NoSuchElementError(Report.text(new Report({ lines: [refusal] }))), Effect.succeed(Array.isReadonlyArrayEmpty(quit)));
    const bound = UndefinedOr.match(wait, { onUndefined: () => Duration.infinity, onDefined: Duration.seconds });
    yield* Target.hold(quit.map(({ instance }) => Target.Hold.Quitting({ instance })));
    const outcomes = yield* Effect.forEach(quit, ({ instance }) => Effect.result(Effect.timeoutOption(server.quit({ instance, ...Struct.pick(given, ['mode']) }), bound)), { concurrency: 'unbounded' });
    const after = Instance.byInstance(yield* SubscriptionRef.get(server.running));
    return new Report({
        lines: Array.zipWith(quit, outcomes, (each, outcome) => {
            const [name, row] = [each.name ?? each.identifier, Option.getOrElse(chosen, () => each)];
            const state = outcome.pipe(
                Result.getSuccess,
                Option.flatten,
                Option.orElse(() => Option.map(Record.get(after, each.instance), Struct.get('state'))),
            );
            const sheet = Option.flatMap(state, Instance.State.matchOrElse({ running: ({ windows }) => Array.findFirst(windows, ({ asking }) => asking) }, Option.none));
            const targeted = Option.exists(held, (aim) => aim.instance === each.instance);
            const answer = Option.map(sheet, ({ kind = 'window', container }) => (targeted && container !== undefined ? new Call({ tool: 'inspect', given: { in: Address.refer(kind, container) } }) : Target.open(name)));
            return new Line({
                head: [name, ...Option.toArray(Option.map(state, Struct.get('_tag')))],
                facts: [
                    ...Result.match(outcome, { onFailure: ({ message }) => [message], onSuccess: Option.match({ onNone: () => [`quit unanswered ${Format.duration(bound)}`], onSome: Array.empty<string> }) }),
                    ...Option.toArray(state).flatMap((now) => print(row, Option.none(), { ...Struct.pick(each, ['instance', 'url']), state: now })),
                ],
                ...Option.getOrUndefined(Option.map(answer, (call) => ({ call }))),
            });
        }),
    });
}, Effect.scoped);

// --- [COMPOSITION] ---------------------------------------------------------------------

const Applications = {
    tool,
    run: Effect.fnUntraced(function* ({ action = 'list', application, ...given }: Tool.Parameters<typeof tool>) {
        const { bundles } = yield* Handle.use((handle) => handle.get(Opens));
        const [named = application] = Record.keys(Record.filter(bundles, (row) => application !== undefined && Array.containsWith(Unmatched.named)([row.name, row.identifier, ...row.aliases], application)));
        return yield* { open: () => open(named, given), close: () => close(named, given), list: () => UndefinedOr.match(named, { onDefined: describe, onUndefined: () => list(given) }) }[action]();
    }),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Applications };
