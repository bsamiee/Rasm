import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import type { Installation } from '@rasm/automation/applications';
import { Automation } from '@rasm/automation/automation';
import { Array, Config, Console, DateTime, Duration, Effect, FileSystem, Layer, Match, Number, Option, Order, Path, Record, Schema } from 'effect';
import { Argument, Command, Flag } from 'effect/cli';
import effect from 'effect/package.json' with { type: 'json' };
import { type Batch, Call, Probe } from '#/cases';
import { compare, type Pair } from '#/compare';
import { HarnessError } from '#/error';
import { answerOf, facts, statements } from '#/facts';
import { Engine, type Outcome, Survey } from '#/indesign/contract';
import { conditionOf, execute, expand, installations, normalize, Results, scriptOf } from '#/indesign/host';
import { precondition } from '#/indesign/precondition';
import { Completion } from '#/results';
import { type Reference, revive } from '#/values';
import npm from '../package.json' with { type: 'json' };

// --- [TYPES] ---------------------------------------------------------------------------

type Row = (typeof Batch.Type.cases)[number];
type Planned = Effect.Success<ReturnType<ReturnType<typeof planning>>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const SURVEY = 'Enumerations, working space profiles, languages, locale, and feature set the installation declares';
const RESULTS = '.results.json';

// --- [MODELS] --------------------------------------------------------------------------

const Facts = facts(Engine);
const Line = Schema.fromJsonString(Schema.Unknown);
const Report = Schema.fromJsonString(Schema.Unknown, { space: 2 });
const Writes = Schema.Record(Schema.String, Schema.Unknown);

// --- [OPERATIONS] ----------------------------------------------------------------------

const stem = (file: string) => Effect.map(Path.Path, (path) => path.join(path.dirname(file), path.basename(file, '.cases.json')));
const shownWith = <A>(row: typeof Outcome.Type, value: A) => (row.alerts.length === 0 ? value : { value, alerts: row.alerts.map(({ text }) => text) });
const expressionOf = (batch: typeof Batch.Type, row: Row) =>
    Match.value(row).pipe(
        Match.when(Schema.is(Probe), ({ evaluate }) => Option.some(evaluate)),
        Match.when(Schema.is(Call), ({ call }) => Option.some(call)),
        Match.orElse(() => scriptOf(batch, row)),
    );
const expectOf = (batch: typeof Batch.Type, row: Row) => row.expect ?? batch.expect ?? (Schema.is(Call)(row) || Schema.is(Probe)(row) ? 'value' : 'read');
const revived = (state: Record.ReadonlyRecord<string, Schema.Json> = {}) => Record.map(state, (held) => revive(held));
const locator =
    (outcome: typeof Outcome.Type) =>
    ({ kind, path }: Reference): unknown =>
        Match.value(kind).pipe(
            Match.when('before', () => revived(outcome.before)[path]),
            Match.when('read', () => revived(outcome.read)[path]),
            Match.orElse(() => ({ $: `${kind}:${path}` })),
        );
const paired = (batch: typeof Batch.Type) => (row: Row, fn: string, outcome: typeof Outcome.Type) =>
    Option.map(answerOf(outcome, null), (answer): Pair => {
        const expect = expectOf(batch, row);
        const keys = row.keys ?? batch.keys ?? [];
        const picked = (state: Record.ReadonlyRecord<string, unknown>) => Record.filter(state, (_, key) => keys.length === 0 || keys.includes(key));
        const before = revived(outcome.before);
        const args = Schema.is(Probe)(row) ? [] : (row.args ?? []);
        return {
            name: row.name,
            fn,
            args: args.map((arg) => revive(arg, locator(outcome))),
            host: 'thrown' in answer ? answer : { value: shownWith(outcome, expect === 'value' ? revive(outcome.value ?? null) : picked(revived(outcome.read))) },
            settled: (rebuilt) => (expect === 'writes' && Schema.is(Writes)(rebuilt) ? picked({ ...before, ...rebuilt }) : rebuilt),
        };
    });
const proven = (outcomes: readonly (typeof Outcome.Type)[]) => (row: Row) =>
    Option.toArray(
        Option.zipWith(
            Option.fromUndefinedOr(Schema.is(Probe)(row) ? undefined : row.fn),
            Array.findFirst(outcomes, ({ name }) => name === row.name),
            (fn, outcome) => ({ row, fn, outcome }),
        ),
    );
const compared = Effect.fn('compared')(function* (root: string, batch: typeof Batch.Type, rows: readonly Row[], file: string, outcomes: readonly (typeof Outcome.Type)[]) {
    const path = yield* Path.Path;
    const rebuilt = yield* Effect.fromOption(Option.fromUndefinedOr(batch.module), () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'case file names no module' }, subject: file }));
    const found = rows.flatMap(proven(outcomes));
    const report = { ...(yield* compare(path.join(root, rebuilt), Array.getSomes(found.map(({ row, fn, outcome }) => paired(batch)(row, fn, outcome))))), unproven: found.filter(({ outcome }) => Option.isNone(answerOf(outcome, null))).map(({ row }) => row.name) };
    yield* Console.log(yield* Schema.encodeEffect(Report)(report));
    yield* report.mismatches.length === 0 ? Effect.void : HarnessError.make({ reason: { _tag: 'Disagreed', mismatches: report.mismatches.length }, subject: rebuilt });
});
const checkedAgainst = (root: string, batch: typeof Batch.Type, rows: readonly Row[], file: string) => (outcome: typeof Results.Type) => Effect.andThen(Console.log(`${outcome.application} ${outcome.version}`), compared(root, batch, rows, file, outcome.cases));
const summary = Effect.fn('summary')(function* (row: typeof Outcome.Type) {
    const shown = Record.filter({ alerts: row.alerts, answered: row.answered, activations: row.activations, leaked: row.leaked, console: row.console, logs: row.logs }, (items) => items.length > 0);
    const completion = Completion.match(row.completion, {
        Ran: () => 'ran',
        Exited: () => 'exited',
        Thrown: ({ message, file, line }) => `thrown ${message}${line === undefined ? '' : ` at ${file ?? ''}:${line}`}`,
        Failed: ({ reason }) => `failed ${reason}`,
    });
    return [row.name, completion, ...(Record.isEmptyRecord(shown) ? [] : [yield* Schema.encodeEffect(Line)(shown)])].join(' | ');
});
const planning = (indexed: readonly (typeof Facts.Fact.Type)[], asking: { readonly identifier: string; readonly version: string }, root: string, file: string, captures: string, only: readonly string[]) =>
    Effect.fnUntraced(function* (survey: typeof Survey.Type) {
        const { batch, rows, engine, sendable, settled } = yield* normalize(root, file, captures, only, precondition(survey));
        const keyOf = (row: Row) => Option.map(expressionOf(batch, row), (expression) => ({ ...asking, engine, host: survey.host, expression, precondition: conditionOf(batch, row) }));
        const answered = (row: Row) => Option.flatMap(keyOf(row), (key) => Option.orElse(Schema.is(Probe)(row) ? Facts.known(indexed, { ...key, question: row.name }) : Option.none(), () => Facts.faulted(indexed, key)));
        const skipped = Array.getSomes(sendable.map(({ row }) => Option.map(answered(row), ({ answer }) => ({ row, answer }))));
        return { batch, rows, engine, keyOf, survey, sendable: sendable.filter(({ row }) => Option.isNone(answered(row))), settled, skipped };
    });
const factOf = (plan: Planned, asked: string) => (row: Row, outcome: typeof Outcome.Type) =>
    Option.flatMap(
        Option.filter(answerOf(outcome, shownWith(outcome, outcome.value ?? null)), () => Schema.is(Probe)(row) || outcome.crashed !== undefined || outcome.hung !== undefined),
        (answer) => Option.map(plan.keyOf(row), (key): typeof Facts.Fact.Type => ({ ...key, question: row.name, answer, asked })),
    );
const reported = Effect.fn('reported')(function* (chosen: Installation, version: string, target: string, plan: Planned, done: Effect.Success<ReturnType<typeof execute<Planned>>>) {
    const fs = yield* FileSystem.FileSystem;
    const header = Option.getOrUndefined(done.header);
    const order = Order.mapInput(Number.Order, ({ name }: typeof Outcome.Type) => plan.rows.findIndex((row) => row.name === name));
    const results = { application: header?.application ?? chosen.name, identifier: chosen.identifier, version, engine: plan.engine, ...(header === undefined ? {} : { state: header.state }), host: plan.survey.host, shared: done.shared, answered: done.answered, cases: Array.sort(done.outcomes, order) };
    yield* fs.writeFileString(`${target}${RESULTS}`, `${yield* Schema.encodeEffect(Results)(results)}\n`);
    yield* Console.log(`${results.application} ${version} ${plan.engine} ${target}${RESULTS}`);
    yield* Effect.forEach(results.cases, (row) => Effect.flatMap(summary(row), Console.log), { discard: true });
});
const atelier = Layer.unwrap(Config.String('ATELIER_PATH').pipe(Effect.map(Automation.layer)));
const released = Effect.fn('released')(function* (chosen: Installation, root: string, file: string, only: readonly string[], limit: Duration.Duration, index: Option.Option<string>, search: (question: string) => readonly string[]) {
    const version = yield* Effect.fromOption(chosen.version, () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'Launch Services reports no version' }, subject: chosen.name }));
    const indexed = yield* Option.match(index, { onNone: () => Effect.succeed([]), onSome: Facts.load });
    const asking = { identifier: chosen.identifier, version };
    const surveyed = { ...asking, question: SURVEY, engine: 'uxpscript' as const, expression: SURVEY };
    const known = Option.flatMap(Facts.known(indexed, surveyed), ({ answer }) => ('value' in answer ? Schema.decodeUnknownOption(Survey)(answer.value) : Option.none()));
    const target = `${yield* stem(file)}.${version}`;
    const planned = planning(indexed, asking, root, file, `${target}.captures`, only);
    const ready = yield* Option.match(known, { onNone: () => Effect.succeedNone, onSome: (survey) => Effect.asSome(planned(survey)) });
    const idle = Option.filter(ready, (held) => held.sendable.length === 0 && held.settled.length === 0);
    const ran = yield* Option.isSome(idle) ? Effect.succeedNone : Effect.asSome(execute(chosen, npm.name, limit, known, (survey) => Option.match(ready, { onSome: Effect.succeed, onNone: () => planned(survey) })));
    const plan = yield* Effect.fromOption(
        Option.orElse(
            Option.map(ran, ({ plan: held }) => held),
            () => idle,
        ),
        () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'no plan' }, subject: file }),
    );
    yield* Effect.forEach(plan.skipped, ({ row, answer }) => Effect.flatMap(Schema.encodeEffect(Line)(answer), (text) => Console.log(`known: ${row.name} | ${text}`)), { discard: true });
    yield* Effect.forEach(
        plan.sendable.filter(({ row }) => Schema.is(Probe)(row)).flatMap(({ row }) => search(row.name).map((line) => `stated: ${row.name} | ${line}`)),
        Console.log,
        { discard: true },
    );
    const asked = DateTime.formatIso(yield* DateTime.now);
    const outcomes = Option.match(ran, { onNone: () => [], onSome: (done) => done.outcomes });
    yield* Option.match(ran, { onNone: () => Effect.void, onSome: (done) => reported(chosen, version, target, plan, done) });
    const made = [
        ...(Option.isSome(ran) && Option.isNone(known) ? [{ ...surveyed, answer: { value: plan.survey }, asked }] : []),
        ...plan.rows.flatMap((row) =>
            Option.toArray(
                Option.flatMap(
                    Array.findFirst(outcomes, ({ name }) => name === row.name),
                    (outcome) => factOf(plan, asked)(row, outcome),
                ),
            ),
        ),
    ];
    yield* Option.match(
        Option.filter(index, () => Array.isArrayNonEmpty(made)),
        { onNone: () => Effect.void, onSome: (held) => Facts.record(held, made) },
    );
    yield* plan.batch.module === undefined || Option.isNone(ran) ? Effect.void : compared(root, plan.batch, plan.rows, file, outcomes);
}, Effect.scoped);

// --- [COMPOSITION] ---------------------------------------------------------------------

const product = Argument.Directory('product', { mustExist: true }).pipe(Argument.withDescription('Product folder holding the scripts and rebuilt modules'));
const casesFile = Argument.File('cases', { mustExist: true }).pipe(Argument.withDescription('Case file, results written beside it as <name>.<version>.results.json'));
const run = Command.make(
    'run',
    {
        product,
        cases: casesFile,
        release: Flag.String('release').pipe(Flag.atLeast(1), Flag.withDefault([]), Flag.withDescription('Version prefix an installation reports, repeated, every installation when absent')),
        only: Flag.String('only').pipe(Flag.atLeast(1), Flag.withDefault([]), Flag.withDescription('Case name to run, repeated')),
        limit: Flag.String('limit').pipe(Flag.withSchema(Schema.DurationFromString), Flag.withDefault(Duration.minutes(2)), Flag.withDescription('Time a case may run with no new record before it records hung')),
        facts: Flag.Path('facts').pipe(Flag.optional, Flag.withDescription('Facts index holding the survey, evaluate answers, and crashed or hung cases, each new one appended')),
        stated: Flag.File('stated', { mustExist: true }).pipe(Flag.atLeast(1), Flag.withDefault([]), Flag.withDescription('Text facts file whose lines matching a question print beside it, repeated')),
    },
    ({ product: folder, cases, release, only, limit, facts: index, stated }) =>
        Effect.gen(function* () {
            const path = yield* Path.Path;
            const search = yield* statements(stated);
            const chosen = yield* installations(release);
            yield* Effect.forEach(chosen, (row) => released(row, path.resolve(folder), path.resolve(cases), only, limit, index, search), { discard: true });
        }).pipe(Effect.provide(atelier)),
);
const checked = Command.make('compare', { product, cases: casesFile }, ({ product: folder, cases }) =>
    Effect.gen(function* () {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const [root, file] = [path.resolve(folder), path.resolve(cases)];
        const { batch, rows } = yield* expand(root, file);
        const prefix = `${path.basename(yield* stem(file))}.`;
        const found = (yield* fs.readDirectory(path.dirname(file))).filter((name) => name.startsWith(prefix) && name.endsWith(RESULTS));
        yield* Effect.forEach(found, (name) => Effect.flatMap(Effect.flatMap(fs.readFileString(path.join(path.dirname(file), name)), Schema.decodeEffect(Results)), checkedAgainst(root, batch, rows, file)));
        yield* found.length === 0 ? HarnessError.make({ reason: { _tag: 'Unavailable', because: `no ${prefix}<version>${RESULTS} beside it` }, subject: file }) : Effect.void;
    }),
);

Command.make('indesign').pipe(Command.withSubcommands([run, checked]), Command.run({ version: effect.version }), Effect.provide(layer), runMain);
