import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Automation } from '@rasm/automation/automation';
import { Array, Config, Console, DateTime, Duration, Effect, FileSystem, Layer, Match, Number, Option, Order, Path, Predicate, Record, Result, Schema, Stdio, UndefinedOr } from 'effect';
import { Argument, Command, Flag } from 'effect/cli';
import effect from 'effect/package.json' with { type: 'json' };
import { type Batch, Call, expand, Probe, type Row, scriptOf } from '#/cases';
import { compare, compared, type Pair, type Reply } from '#/compare';
import { HarnessError } from '#/error';
import { answerOf, facts, statements } from '#/facts';
import { merged } from '#/precondition';
import { type Answer, Completion, Outcome, results } from '#/results';
import { type Any, type Decoding, type Done, execute, type Host, type Installed, installations, type Plan, type Prepared, type Rebuilt } from '#/session';
import { prepared } from '#/source';
import { bury, JsonObject, type Reference, revive } from '#/values';
import npm from './package.json' with { type: 'json' };

// --- [TYPES] ---------------------------------------------------------------------------

type AnyHost = Host<Any, Any, Any>;
type Facts = ReturnType<typeof facts<Any>>;
type Fact = Facts['Fact']['Type'];
type Item = Prepared<unknown> & { readonly row: Row };
interface Planned extends Plan<unknown, unknown> {
    readonly sendable: readonly Item[];
    readonly skipped: readonly { readonly item: Item; readonly answer: typeof Answer.Type }[];
}
interface Asking {
    readonly identifier: string;
    readonly version: string;
    readonly engine: unknown;
}
interface Evidence {
    readonly answer: typeof Answer.Type;
    readonly before: typeof JsonObject.Type;
    readonly read: typeof JsonObject.Type;
}
interface Hosted {
    readonly chosen: Installed;
    readonly items: readonly Item[];
    readonly batch: typeof Batch.Type;
}
interface Verdict {
    readonly mismatches: number;
    readonly failed: Option.Option<string>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const CASES = '.cases.json';
const RESULTS = '.results.json';
const PROOF = 'proof';
const LABEL = npm.name.replaceAll(/\W+/gu, '-').replace(/^-/u, '');

// --- [MODELS] --------------------------------------------------------------------------

const Line = Schema.fromJsonString(Schema.Unknown);
const Alerts = Schema.Array(Schema.Struct({ text: Schema.String }));
const resultsOf = (host: AnyHost) => results(host.Engine, { state: Schema.optionalKey(Schema.Json.annotate({ description: 'Instance state at batch start' })), host: Schema.optionalKey(Schema.Json.annotate({ description: 'Install state deciding answers beside the version' })) }, Outcome);
const Callable = Schema.declare(Predicate.isFunction);
const Declared = Schema.declare(Schema.isSchema);
const Shape = Schema.Struct({
    name: Schema.String,
    Engine: Declared,
    survey: Schema.Struct({ question: Schema.String, Schema: Declared, host: Callable }),
    Case: Declared,
    shown: Schema.Array(Schema.String),
    engine: Callable,
    precondition: Callable,
    placed: Callable,
    refusal: Callable,
    installed: Schema.declare(Effect.isEffect),
    launch: Callable,
    opened: Callable,
    send: Callable,
    records: Callable,
    quit: Callable,
    digest: Callable,
    compare: Schema.TaggedUnion({ Imported: {}, Hosted: { run: Callable } }),
}).annotate({ description: 'Host value a host module exports' });
const Loaded = Schema.Struct({ Host: Schema.declare((value: unknown): value is AnyHost => Schema.is(Shape)(value)) });
const atelier = Layer.unwrap(Config.String('ATELIER_PATH').pipe(Effect.map(Automation.layer)));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [PATHS]
const hostOf = Effect.fn('hostOf')(function* (name: string) {
    const file = new URL(`./${name}/host.ts`, import.meta.url);
    const loaded = yield* Effect.tryPromise({ try: (): Promise<unknown> => import(file.href), catch: HarnessError.refused(file.pathname) });
    return (yield* Schema.decodeUnknownEffect(Loaded)(loaded)).Host;
});
const stem = (path: Path.Path, file: string) => path.join(path.dirname(file), path.basename(file, CASES));
const ancestors = (path: Path.Path, folder: string): readonly string[] => (path.dirname(folder) === folder ? [folder] : [folder, ...ancestors(path, path.dirname(folder))]);
const productOf = (path: Path.Path, file: string) =>
    Option.match(
        Array.findFirst(ancestors(path, path.dirname(file)), (folder) => path.basename(folder) === PROOF),
        { onNone: () => path.dirname(file), onSome: (proof) => path.dirname(proof) },
    );
const casesUnder = Effect.fn('casesUnder')(function* (target: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { type } = yield* fs.stat(target);
    const found = type === 'Directory' ? (yield* fs.readDirectory(target, { recursive: true })).filter((file) => file.endsWith(CASES)).map((file) => path.join(target, file)) : [target];
    return found.map((file) => path.resolve(file));
});
const targets = (paths: readonly string[]) => Effect.map(Effect.forEach(paths, casesUnder), (files) => Array.dedupe(files.flat()).toSorted());
const factsOf = Effect.fn('factsOf')(function* (name: string, product: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const segments = product.split(path.sep);
    const folder = Option.map(
        Array.findFirstIndex(segments, (segment, index) => segment === 'applications' && segments[index + 1] === name),
        (index) => path.join(segments.slice(0, index + 2).join(path.sep), 'facts'),
    );
    const stated = yield* Option.match(folder, { onNone: () => Effect.succeed([]), onSome: (held) => Effect.map(fs.readDirectory(held), (files) => files.filter((file) => file.endsWith('.txt')).map((file) => path.join(held, file))) });
    return { index: Option.map(folder, (held) => path.join(held, `${name}-host.json`)), search: yield* statements(stated) };
});

// --- [PLANNING]
const expressionOf = (batch: typeof Batch.Type, row: Row) =>
    Match.value(row).pipe(
        Match.when(Schema.is(Probe), ({ evaluate }) => Option.some(evaluate)),
        Match.when(Schema.is(Call), ({ call }) => Option.some(call)),
        Match.orElse(() => scriptOf(batch, row)),
    );
const argsOf = (row: Row) => (Schema.is(Probe)(row) ? [] : (row.args ?? []));
const prepare = (host: AnyHost, product: string, captures: string, batch: typeof Batch.Type, Precondition: Decoding<unknown>, engine: unknown) =>
    Effect.fn('prepare')(function* (row: Row, index: number) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const folder = path.join(captures, String(index));
        yield* fs.makeDirectory(folder, { recursive: true });
        const decoded = yield* Schema.decodeEffect(Precondition)(merged(batch.precondition ?? {}, row.precondition ?? {}));
        const { precondition, copies } = yield* host.placed(decoded, folder, product);
        const keyworded = UndefinedOr.match({ onUndefined: () => ({}), onDefined: (held: NonNullable<typeof Call.Type.keywords>) => ({ keywords: held }) });
        const located = (file: string | null) => Option.getOrNull(Option.map(Option.fromNullOr(file), (held) => path.join(product, held)));
        const kind = Match.value(row).pipe(
            Match.when(Schema.is(Probe), ({ evaluate }) => ({ _tag: 'Probe' as const, evaluate })),
            Match.when(Schema.is(Call), ({ call, args, keywords }) => ({ _tag: 'Call' as const, call, args: args.map((arg) => bury(arg)), ...keyworded(keywords) })),
            Match.orElse(({ args = [], keywords }) => ({ _tag: 'Run' as const, args: args.map((arg) => bury(arg)), ...keyworded(keywords) })),
        );
        const lines = Option.flatMap(Option.liftPredicate(row, Schema.is(Call)), ({ lines: held }) => Option.fromUndefinedOr(held));
        const loaded = yield* Effect.transposeOption(Option.map(scriptOf(batch, row), (script) => prepared(path.join(product, script), lines)));
        const source = Option.map(loaded, ({ source: held }) => held);
        yield* kind._tag === 'Run' && Option.isNone(source) ? HarnessError.make({ reason: { _tag: 'Unavailable', because: 'no script for the run case or batch' }, subject: row.name }) : Effect.void;
        const answers = Record.filter(
            {
                dialogs: row.dialogs,
                windows: row.windows,
                confirms: row.confirms,
                prompts: row.prompts,
                presses: row.presses,
                commands: row.commands,
                files: UndefinedOr.map(row.files, (files) => files.map(located)),
                plugin: UndefinedOr.map(batch.plugin, (plugin) => path.join(product, plugin)),
                source: Option.getOrUndefined(source),
            },
            Predicate.isNotUndefined,
        );
        const hosted = yield* Schema.decodeEffect(host.Case)({ ...kind, name: row.name, precondition, read: { ...batch.read, ...row.read }, folder, interact: (row.presses ?? []).length > 0, ...answers });
        return yield* Option.match(
            host.refusal(
                engine,
                hosted,
                Option.exists(loaded, ({ binary }) => binary),
            ),
            {
                onNone: () => Effect.succeed<Item>({ hosted, row, name: row.name, folder, map: Option.match(loaded, { onNone: () => [], onSome: ({ map }) => map }), presses: row.presses ?? [], copies }),
                onSome: (reason) => HarnessError.make({ reason: { _tag: 'Unavailable', because: reason }, subject: row.name }),
            },
        );
    });
const planning = (host: AnyHost, Facts: Facts, indexed: readonly Fact[], asking: Asking, product: string, captures: string, batch: typeof Batch.Type, rows: readonly Row[]) => {
    const failed = (row: Row) => (error: HarnessError | Schema.SchemaError) => Effect.map(Effect.orDie(Schema.decodeEffect(Outcome)({ name: row.name, completion: { _tag: 'Failed', reason: error.message } })), Result.fail);
    const keyOf = (survey: unknown) => (row: Row) =>
        Option.map(expressionOf(batch, row), (expression) => ({ ...asking, question: row.name, expression, args: argsOf(row), precondition: merged(batch.precondition ?? {}, row.precondition ?? {}), ...Option.match(host.survey.host(survey), { onNone: () => ({}), onSome: (held) => ({ host: held }) }) }));
    const planned = Effect.fnUntraced(function* (survey: unknown) {
        const fs = yield* FileSystem.FileSystem;
        yield* fs.remove(captures, { recursive: true, force: true });
        const made = yield* Effect.forEach(rows, (row, index) => prepare(host, product, captures, batch, host.precondition(survey), asking.engine)(row, index).pipe(Effect.map(Result.succeed), Effect.catchTags({ HarnessError: failed(row), SchemaError: failed(row) })));
        const answered = (row: Row) => Option.flatMap(keyOf(survey)(row), (key) => Option.orElse(Facts.known(indexed, key), () => Facts.faulted(indexed, key)));
        const sendable = Array.getSuccesses(made);
        const skipped = Array.getSomes(sendable.map((item) => Option.map(answered(item.row), ({ answer }) => ({ item, answer }))));
        return { engine: asking.engine, sendable: sendable.filter((item) => !skipped.some((held) => held.item === item)), settled: Array.getFailures(made), skipped } satisfies Planned;
    });
    return { planned, keyOf };
};

// --- [COMPARING]
const shownWith = (row: typeof Outcome.Type, value: Schema.Json): Schema.Json =>
    Option.match(
        Option.filter(Schema.decodeUnknownOption(Alerts)(row.alerts), (alerts) => alerts.length > 0),
        { onNone: () => value, onSome: (alerts) => ({ value, alerts: alerts.map(({ text }) => text) }) },
    );
const revived = Record.map((held: Schema.Json) => revive(held));
const locator =
    (evidence: Evidence) =>
    ({ kind, path }: Reference): unknown =>
        Match.value(kind).pipe(
            Match.when('before', () => revived(evidence.before)[path]),
            Match.when('read', () => revived(evidence.read)[path]),
            Match.orElse(() => ({ $: `${kind}:${path}` })),
        );
const evidenceOf = (row: typeof Outcome.Type): Option.Option<Evidence> => Option.map(answerOf(row, shownWith(row, row.value ?? null)), (answer) => ({ answer, before: row.before ?? {}, read: row.read ?? {} }));
const paired = (batch: typeof Batch.Type, row: Row, fn: string, evidence: Evidence): Pair => {
    const expect = row.expect ?? batch.expect ?? (Schema.is(Call)(row) || Schema.is(Probe)(row) ? 'value' : 'read');
    const keys = row.keys ?? batch.keys ?? [];
    const picked = Record.filter((_: unknown, key: string) => keys.length === 0 || keys.includes(key));
    const before = revived(evidence.before);
    return {
        name: row.name,
        fn,
        args: argsOf(row).map((arg) => revive(arg, locator(evidence))),
        host: 'thrown' in evidence.answer ? evidence.answer : { value: expect === 'value' ? revive(evidence.answer.value) : picked(revived(evidence.read)) },
        settled: (rebuilt) => (expect === 'writes' && Schema.is(JsonObject)(rebuilt) ? picked({ ...before, ...rebuilt }) : rebuilt),
    };
};
const pairsOf = (batch: typeof Batch.Type, rows: readonly Row[], evidence: (row: Row) => Option.Option<Evidence>) => {
    const proven = rows.flatMap((row) => Option.toArray(Option.map(Option.fromUndefinedOr(Schema.is(Probe)(row) ? undefined : row.fn), (fn) => ({ row, fn }))));
    return {
        pairs: proven.flatMap(({ row, fn }) => Option.toArray(Option.map(evidence(row), (held) => paired(batch, row, fn, held)))),
        unproven: proven.filter(({ row }) => Option.isNone(evidence(row))).map(({ row }) => row.name),
    };
};
const callOf = (items: readonly Item[], module: string) => (pair: Pair) =>
    Option.map(
        Array.findFirst(items, ({ name }) => name === pair.name),
        ({ hosted, row }): Rebuilt<unknown> => ({ hosted, fn: pair.fn, args: argsOf(row), module }),
    );
const replyOf = (row: typeof Outcome.Type) => Option.map(answerOf(row, shownWith(row, row.value ?? null)), (answer): readonly [string, Reply] => [row.name, 'value' in answer ? Result.succeed(revive(answer.value)) : Result.fail(answer.thrown)]);
const uninstalled = (module: string) => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'runs the module in the host, which is not installed' }, subject: module });
const comparing = (host: AnyHost, hosted: Option.Option<Hosted>, module: string, pairs: readonly Pair[], unproven: readonly string[], scratch: string, limit: Duration.Duration) =>
    Match.value(host.compare).pipe(
        Match.when({ _tag: 'Imported' }, () => compare(module, pairs, unproven)),
        Match.when({ _tag: 'Hosted' }, ({ run }) =>
            Effect.fromOption(hosted, () => uninstalled(module)).pipe(
                Effect.flatMap(({ chosen, items, batch }) => run(chosen, Array.getSomes(pairs.map(callOf(items, module))), scratch, limit, batch)),
                Effect.map((rebuilt) => compared(module, pairs, new Map(Array.getSomes(rebuilt.map(replyOf))), unproven)),
            ),
        ),
        Match.exhaustive,
    );

// --- [REPORTING]
const order = (rows: readonly Row[]) => Order.mapInput(Number.Order, ({ name }: typeof Outcome.Type) => rows.findIndex((row) => row.name === name));
const summary = (host: AnyHost) =>
    Effect.fn('summary')(function* (row: typeof Outcome.Type) {
        const shown = {
            ...Record.filter({ answered: row.answered, leaked: row.leaked, ...Record.fromIterableWith(host.shown, (key) => [key, row[key]] as const) }, (items) => Array.isArray(items) && items.length > 0),
            ...Record.filter({ crashed: row.crashed, hung: row.hung, dialog: row.dialog }, Predicate.isNotUndefined),
        };
        const completion = Completion.match(row.completion, {
            Ran: () => 'ran',
            Exited: () => 'exited',
            Thrown: ({ message, file, line }) => `thrown ${message}${line === undefined ? '' : ` at ${file ?? ''}:${line}`}`,
            Failed: ({ reason }) => `failed ${reason}`,
        });
        return [row.name, completion, ...(Record.isEmptyRecord(shown) ? [] : [yield* Schema.encodeEffect(Line)(shown)])].join(' | ');
    });
const counted = ({ agreed, mismatches, unproven }: { readonly agreed: number; readonly mismatches: readonly unknown[]; readonly unproven: readonly string[] }) => `${agreed} agreed, ${mismatches.length} mismatched, ${unproven.length} unproven`;
const proved = Effect.fn('proved')(function* (host: AnyHost, chosen: Installed, file: string, only: readonly string[], limit: Duration.Duration) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const product = productOf(path, file);
    const Facts = facts(host.Engine);
    const Results = resultsOf(host);
    const { index, search } = yield* factsOf(host.name, product);
    const indexed = yield* Option.match(index, { onNone: () => Effect.succeed([]), onSome: Facts.load });
    const { batch, rows: every } = yield* expand(product, file);
    yield* Schema.decodeEffect(Schema.Array(Schema.Literals(every.map(({ name }) => name))))(only);
    const rows = every.filter(({ name }) => only.length === 0 || only.includes(name));
    const engine = yield* Effect.fromResult(host.engine(Array.dedupe(Array.getSomes(rows.map((row) => scriptOf(batch, row))))));
    const asking: Asking = { identifier: chosen.identifier, version: chosen.version, engine };
    const surveyed = { ...asking, question: host.survey.question, expression: host.survey.question };
    const known = Option.flatMap(Facts.known(indexed, surveyed), ({ answer }) => ('value' in answer ? Schema.decodeOption(host.survey.Schema)(answer.value) : Option.none()));
    const target = `${stem(path, file)}.${chosen.version}`;
    const scratch = yield* fs.makeTempDirectoryScoped({ prefix: `${host.name}-harness-` });
    const { planned, keyOf } = planning(host, Facts, indexed, asking, product, `${target}.captures`, batch, rows);
    const done: Done<unknown, unknown, unknown, Planned> = yield* execute(host, chosen, scratch, LABEL, limit, known, planned, batch);
    const { plan } = done;
    yield* Effect.forEach(plan.skipped, ({ item, answer }) => Effect.flatMap(Schema.encodeEffect(Line)(answer), (text) => Console.log(`known: ${item.name} | ${text}`)), { discard: true });
    yield* Effect.forEach(
        plan.sendable.filter(({ row }) => Schema.is(Probe)(row)).flatMap(({ row }) => search(row.name).map((line) => `stated: ${row.name} | ${line}`)),
        Console.log,
        { discard: true },
    );
    const asked = DateTime.formatIso(yield* DateTime.now);
    const outcomes = Array.sort(done.outcomes, order(rows));
    const evidence = (row: Row) =>
        Option.orElse(
            Option.flatMap(
                Array.findFirst(outcomes, ({ name }) => name === row.name),
                evidenceOf,
            ),
            () =>
                Option.map(
                    Array.findFirst(plan.skipped, ({ item }) => item.name === row.name),
                    ({ answer }): Evidence => ({ answer, before: {}, read: {} }),
                ),
        );
    const { pairs, unproven } = pairsOf(batch, rows, evidence);
    const section = yield* UndefinedOr.match(batch.module, {
        onUndefined: () => Effect.succeedNone,
        onDefined: (module) => Effect.asSome(comparing(host, Option.some({ chosen, items: [...plan.sendable, ...plan.skipped.map(({ item }) => item)], batch }), path.join(product, module), pairs, unproven, scratch, limit)),
    });
    const header = Option.getOrUndefined(done.header);
    const hosting = Option.match(host.survey.host(done.survey), { onNone: () => ({}), onSome: (held) => ({ host: held }) });
    const written = {
        application: header?.application ?? chosen.name,
        identifier: chosen.identifier,
        version: chosen.version,
        engine,
        ...(header?.state === undefined ? {} : { state: header.state }),
        ...hosting,
        shared: done.shared,
        ...Option.match(done.failed, { onNone: () => ({}), onSome: (failed) => ({ failed }) }),
        answered: done.answered,
        cases: outcomes,
        ...Option.match(section, { onNone: () => ({}), onSome: (held) => ({ compare: held }) }),
    };
    yield* Option.isNone(done.header) && outcomes.length === 0 ? Effect.void : Effect.andThen(fs.writeFileString(`${target}${RESULTS}`, `${yield* Schema.encodeEffect(Results)(written)}\n`), Console.log(`${written.application} ${chosen.version} ${target}${RESULTS}`));
    yield* Effect.forEach([...(done.shared.length === 0 ? [] : [`shared: ${done.shared.join(', ')}`]), ...Option.toArray(Option.map(done.failed, (failed) => `failed: ${failed}`))], Console.log, { discard: true });
    yield* Effect.forEach(outcomes, (row) => Effect.flatMap(summary(host)(row), Console.log), { discard: true });
    yield* Option.match(section, { onNone: () => Effect.void, onSome: (held) => Console.log(`compare: ${counted(held)}`) });
    const factOf = (row: Row) =>
        Option.flatMap(
            Array.findFirst(outcomes, ({ name }) => name === row.name),
            (held) => Option.zipWith(answerOf(held, shownWith(held, held.value ?? null)), keyOf(done.survey)(row), (answer, key): Fact => ({ ...key, answer, asked })),
        );
    const made: readonly Fact[] = [...(Option.isSome(done.header) && Option.isNone(known) ? [{ ...surveyed, ...hosting, answer: { value: bury(yield* Schema.encodeEffect(host.survey.Schema)(done.survey)) }, asked }] : []), ...rows.flatMap((row) => Option.toArray(factOf(row)))];
    yield* Option.match(
        Option.filter(index, () => made.length > 0),
        { onNone: () => Effect.void, onSome: (held) => Facts.record(held, made) },
    );
    return { mismatches: Option.match(section, { onNone: () => 0, onSome: ({ mismatches }) => mismatches.length }), failed: done.failed } satisfies Verdict;
}, Effect.scoped);
const recompared = Effect.fn('recompared')(function* (host: AnyHost, file: string, override: Option.Option<string>, limit: Duration.Duration) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const product = productOf(path, file);
    const Facts = facts(host.Engine);
    const Results = resultsOf(host);
    const { batch, rows } = yield* expand(product, file);
    const module = yield* Effect.fromOption(
        Option.orElse(override, () => Option.map(Option.fromUndefinedOr(batch.module), (held) => path.join(product, held))),
        () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'case file names no module' }, subject: file }),
    );
    const prefix = `${path.basename(stem(path, file))}.`;
    const found = (yield* fs.readDirectory(path.dirname(file))).filter((name) => name.startsWith(prefix) && name.endsWith(RESULTS));
    yield* found.length === 0 ? HarnessError.make({ reason: { _tag: 'Unavailable', because: `no ${prefix}<version>${RESULTS} beside it` }, subject: file }) : Effect.void;
    const hosting = Effect.fnUntraced(function* (recorded: typeof Results.Type, scratch: string, target: string) {
        const { index } = yield* factsOf(host.name, product);
        const indexed = yield* Option.match(index, { onNone: () => Effect.succeed([]), onSome: Facts.load });
        const asking: Asking = { identifier: recorded.identifier, version: recorded.version, engine: recorded.engine };
        const survey = yield* Effect.fromOption(
            Option.flatMap(Facts.known(indexed, { ...asking, question: host.survey.question }), ({ answer }) => ('value' in answer ? Schema.decodeOption(host.survey.Schema)(answer.value) : Option.none())),
            () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'facts index holds no survey of the recorded release, prove it first' }, subject: target }),
        );
        const [chosen] = yield* Effect.flatMap(host.installed, (installed) => installations(installed, [recorded.version], host.name));
        const { sendable } = yield* planning(host, Facts, [], asking, product, path.join(scratch, 'captures'), batch, rows).planned(survey);
        return { chosen, items: sendable, batch } satisfies Hosted;
    });
    const judged = Effect.fnUntraced(function* (name: string) {
        const target = path.join(path.dirname(file), name);
        const recorded = yield* Effect.flatMap(fs.readFileString(target), Schema.decodeEffect(Results));
        const scratch = yield* fs.makeTempDirectoryScoped({ prefix: `${host.name}-compare-` });
        const evidence = (row: Row) =>
            Option.flatMap(
                Array.findFirst(recorded.cases, ({ name: held }) => held === row.name),
                evidenceOf,
            );
        const { pairs, unproven } = pairsOf(batch, rows, evidence);
        const hosted = yield* host.compare._tag === 'Hosted' ? Effect.asSome(hosting(recorded, scratch, target)) : Effect.succeedNone;
        const section = yield* comparing(host, hosted, module, pairs, unproven, scratch, limit);
        yield* fs.writeFileString(target, `${yield* Schema.encodeEffect(Results)({ ...recorded, compare: section })}\n`);
        yield* Console.log(`${recorded.application} ${recorded.version} ${target} | compare: ${counted(section)}`);
        return { mismatches: section.mismatches.length, failed: Option.fromUndefinedOr(recorded.failed) } satisfies Verdict;
    }, Effect.scoped);
    return yield* Effect.forEach(found, judged);
});
const judged = (verdicts: readonly Verdict[], subject: string) => {
    const mismatches = verdicts.reduce((total, { mismatches: held }) => total + held, 0);
    const failed = Array.getSomes(verdicts.map(({ failed: held }) => held));
    return mismatches > 0 ? HarnessError.make({ reason: { _tag: 'Disagreed', mismatches }, subject }) : Array.match(failed, { onEmpty: () => Effect.void, onNonEmpty: (held) => HarnessError.make({ reason: { _tag: 'Unavailable', because: held.join('; ') }, subject }) });
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const cases = Argument.Path('target', { mustExist: true }).pipe(Argument.atLeast(1), Argument.withDescription('Product folder whose **/*.cases.json files run, or one case file, results written beside each as <name>.<version>.results.json'));
const proving = (host: AnyHost, chosen: readonly Installed[], only: readonly string[], limit: Duration.Duration) => (file: string) => Effect.forEach(chosen, (installed) => proved(host, installed, file, only, limit));
const limited = Flag.String('limit').pipe(
    Flag.withSchema(Schema.DurationFromString),
    Flag.withDefault(Duration.minutes(2)),
    Flag.withDescription('Time a launch may take to respond, a case may run with no new record before it records hung, and the quit at batch end may take before the instance is reported left running'),
);
const prove = (name: string) =>
    Command.make(
        'prove',
        {
            targets: cases,
            release: Flag.String('release').pipe(Flag.atLeast(1), Flag.withDefault([]), Flag.withDescription('Version prefix an installation reports, repeated, every installation when absent')),
            only: Flag.String('only').pipe(Flag.atLeast(1), Flag.withDefault([]), Flag.withDescription('Case name to run, repeated')),
            limit: limited,
        },
        ({ targets: paths, release, only, limit }) =>
            Effect.gen(function* () {
                const host = yield* hostOf(name);
                const [files, chosen] = yield* Effect.all([targets(paths), Effect.flatMap(host.installed, (rows) => installations(rows, release, host.name))]);
                const verdicts = yield* Effect.forEach(files, proving(host, chosen, only, limit));
                yield* judged(verdicts.flat(), name);
            }),
    );
const checked = (name: string) =>
    Command.make('compare', { targets: cases, module: Flag.Path('module', { mustExist: true }).pipe(Flag.optional, Flag.withDescription('Module compared in place of the batch module, a distilled module among them')), limit: limited }, ({ targets: paths, module, limit }) =>
        Effect.gen(function* () {
            const [host, path, files] = yield* Effect.all([hostOf(name), Path.Path, targets(paths)]);
            const override = Option.map(module, (held) => path.resolve(held));
            const verdicts = yield* Effect.forEach(files, (file) => recompared(host, file, override, limit));
            yield* judged(verdicts.flat(), name);
        }),
    );
Effect.gen(function* () {
    const args = yield* (yield* Stdio.Stdio).args;
    const name = yield* Effect.fromOption(Array.head(args), () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'first argument names the host' }, subject: LABEL }));
    return yield* Command.runWith(Command.make('harness').pipe(Command.withSubcommands([prove(name), checked(name)])), { version: effect.version })(Array.drop(args, 1));
}).pipe(Effect.provide(Layer.provideMerge(atelier, layer)), runMain);
