import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import type { Installation } from '@rasm/automation/applications';
import { Automation } from '@rasm/automation/automation';
import { Array, Config, Console, DateTime, Duration, Effect, FileSystem, Layer, Match, Number, Option, Order, Path, Record, Schema } from 'effect';
import { Argument, Command, Flag } from 'effect/cli';
import effect from 'effect/package.json' with { type: 'json' };
import { Engine, Host, Model, type Outcome } from '#/acrobat/contract';
import { conditionOf, execute, installations, lockdown, normalize, Results, sandbox, writtenIn } from '#/acrobat/host';
import { type Batch, Call, Probe } from '#/cases';
import { compare, type Pair } from '#/compare';
import { HarnessError } from '#/error';
import { answerOf, facts, statements } from '#/facts';
import { expand, scriptOf } from '#/indesign/host';
import { Completion } from '#/results';
import { type Reference, revive } from '#/values';

// --- [TYPES] ---------------------------------------------------------------------------

type Row = (typeof Batch.Type.cases)[number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const MODEL = 'Member names per object kind, signatures of answered members, and dialog menu items the installation declares';
const HOST = 'Viewer type, variation, UI language, plug-ins, and FeatureLockDown digest of the instance';
const RESULTS = '.results.json';

// --- [MODELS] --------------------------------------------------------------------------

const Facts = facts(Engine);
const Line = Schema.fromJsonString(Schema.Unknown);
const Report = Schema.fromJsonString(Schema.Unknown, { space: 2 });
const Writes = Schema.Record(Schema.String, Schema.Unknown);

// --- [OPERATIONS] ----------------------------------------------------------------------

const stem = (file: string) => Effect.map(Path.Path, (path) => path.join(path.dirname(file), path.basename(file, '.cases.json')));
const expressionOf = (batch: typeof Batch.Type, row: Row) =>
    Match.value(row).pipe(
        Match.when(Schema.is(Probe), ({ evaluate }) => Option.some(evaluate)),
        Match.when(Schema.is(Call), ({ call }) => Option.some(call)),
        Match.orElse(() => scriptOf(batch, row)),
    );
const locator =
    (outcome: typeof Outcome.Type) =>
    ({ kind, path }: Reference): unknown =>
        Match.value(kind).pipe(
            Match.when('before', () => outcome.before?.[path]),
            Match.when('read', () => outcome.read?.[path]),
            Match.orElse(() => ({ $: `${kind}:${path}` })),
        );
const paired = (expect: NonNullable<typeof Batch.Type.expect>, keys: readonly string[]) => (row: Row, fn: string, outcome: typeof Outcome.Type) =>
    Option.map(answerOf(outcome, null), (answer): Pair => {
        const picked = Record.filter((_: unknown, key: string) => keys.length === 0 || keys.includes(key));
        const before = Record.map(outcome.before ?? {}, (held) => revive(held));
        return {
            name: row.name,
            fn,
            args: (Schema.is(Probe)(row) ? [] : (row.args ?? [])).map((arg) => revive(arg, locator(outcome))),
            host: 'thrown' in answer ? answer : { value: expect === 'value' ? revive(outcome.value ?? null) : picked(Record.map(outcome.read ?? {}, (held) => revive(held))) },
            settled: (rebuilt) => (expect === 'writes' && Schema.is(Writes)(rebuilt) ? picked({ ...before, ...rebuilt }) : rebuilt),
        };
    });
const compared = Effect.fn('compared')(function* (root: string, batch: typeof Batch.Type, rows: readonly Row[], file: string, outcomes: readonly (typeof Outcome.Type)[]) {
    const path = yield* Path.Path;
    const rebuilt = yield* Effect.fromOption(Option.fromUndefinedOr(batch.module), () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'case file names no module' }, subject: file }));
    const found = rows.flatMap((row) =>
        Option.toArray(
            Option.zipWith(
                Option.fromUndefinedOr(Schema.is(Probe)(row) ? undefined : row.fn),
                Array.findFirst(outcomes, ({ name }) => name === row.name),
                (fn, outcome) => ({ row, fn, outcome }),
            ),
        ),
    );
    const report = {
        ...(yield* compare(path.join(root, rebuilt), Array.getSomes(found.map(({ row, fn, outcome }) => paired(row.expect ?? batch.expect ?? (Schema.is(Call)(row) || Schema.is(Probe)(row) ? 'value' : 'read'), row.keys ?? batch.keys ?? [])(row, fn, outcome))))),
        unproven: found.filter(({ outcome }) => Option.isNone(answerOf(outcome, null))).map(({ row }) => row.name),
    };
    yield* Console.log(yield* Schema.encodeEffect(Report)(report));
    yield* report.mismatches.length === 0 ? Effect.void : HarnessError.make({ reason: { _tag: 'Disagreed', mismatches: report.mismatches.length }, subject: rebuilt });
});
const summary = Effect.fn('summary')(function* (row: typeof Outcome.Type) {
    const shown = Record.filter({ alerts: row.alerts, confirms: row.confirms, prompts: row.prompts, files: row.files, windows: row.windows, activations: row.activations, writes: row.writes, events: row.events, answered: row.answered, leaked: row.leaked, console: row.console }, (items) => items.length > 0);
    const completion = Completion.match(row.completion, { Ran: () => 'ran', Exited: () => 'exited', Thrown: ({ message, file, line }) => `thrown ${message}${line === undefined ? '' : ` at ${file ?? ''}:${line}`}`, Failed: ({ reason }) => `failed ${reason}` });
    return [row.name, completion, ...(Record.isEmptyRecord(shown) ? [] : [yield* Schema.encodeEffect(Line)(shown)])].join(' | ');
});
const atelier = Layer.unwrap(Config.String('ATELIER_PATH').pipe(Effect.map(Automation.layer)));
const released = Effect.fn('released')(function* (chosen: Installation, root: string, file: string, only: readonly string[], limit: Duration.Duration, index: Option.Option<string>, search: (question: string) => readonly string[]) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const version = yield* Effect.fromOption(chosen.version, () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'Launch Services reports no version' }, subject: chosen.name }));
    const indexed = yield* Option.match(index, { onNone: () => Effect.succeed([]), onSome: Facts.load });
    const asking = { identifier: chosen.identifier, version, engine: Engine.literal };
    const asked = (question: string) => ({ ...asking, question, expression: question });
    const stored = <S extends Schema.Top & Schema.Decoder<unknown>>(question: string, schema: S) => Option.flatMap(Facts.known(indexed, asked(question)), ({ answer }) => ('value' in answer ? Schema.decodeUnknownOption(schema)(answer.value) : Option.none()));
    const known = stored(MODEL, Model);
    const sealed = yield* lockdown();
    const host = Option.filter(stored(HOST, Host), (held) => Option.getOrUndefined(sealed) === held.lockdown);
    const target = `${yield* stem(file)}.${version}`;
    const cases = yield* fs.makeTempDirectoryScoped({ directory: yield* sandbox(yield* path.fromFileUrl(chosen.url)), prefix: 'rasm-harnesses-' });
    const plan = yield* normalize(root, file, cases, only);
    const keyOf = (row: Row, state: Option.Option<typeof Host.Type>) => Option.map(expressionOf(plan.batch, row), (expression) => ({ ...asking, ...Option.match(state, { onNone: () => ({}), onSome: (held) => ({ host: held }) }), expression, precondition: conditionOf(plan.batch, row) }));
    const answered = (row: Row) =>
        Option.flatMap(
            Option.flatMap(host, (held) => keyOf(row, Option.some(held))),
            (key) => Option.orElse(Schema.is(Probe)(row) ? Facts.known(indexed, { ...key, question: row.name }) : Option.none(), () => Facts.faulted(indexed, key)),
        );
    const skipped = Array.getSomes(plan.sendable.map((item) => Option.map(answered(item.row), ({ answer }) => ({ item, answer }))));
    const sendable = plan.sendable.filter(({ row }) => Option.isNone(answered(row)));
    yield* Effect.forEach(skipped, ({ item, answer }) => Effect.flatMap(Schema.encodeEffect(Line)(answer), (text) => Console.log(`known: ${item.row.name} | ${text}`)), { discard: true });
    yield* Effect.forEach(
        sendable.filter(({ row }) => Schema.is(Probe)(row)).flatMap(({ row }) => search(row.name).map((line) => `stated: ${row.name} | ${line}`)),
        Console.log,
        { discard: true },
    );
    if (sendable.length === 0 && plan.settled.length === 0 && Option.isSome(known)) {
        return;
    }
    const done = yield* execute(chosen, limit, known, sendable, { host, cases: skipped.map(({ item }) => item) });
    const ran = [...sendable, ...skipped.map(({ item }) => item)].filter(({ hosted }) => done.outcomes.some(({ name }) => name === hosted.name));
    const written = new Map(yield* Effect.forEach(ran, ({ hosted }) => Effect.map(writtenIn(hosted.folder, `${target}.captures`, done.copied, done.kept.get(hosted.name) ?? []), (rows) => [hosted.name, rows] as const)));
    const order = Order.mapInput(Number.Order, ({ name }: typeof Outcome.Type) => plan.rows.findIndex((row) => row.name === name));
    const outcomes = Array.sort(
        [...plan.settled, ...done.outcomes].map((row) => ({ ...row, written: written.get(row.name) ?? [] })),
        order,
    );
    const results = { application: chosen.name, identifier: chosen.identifier, version, engine: Engine.literal, host: done.host, answered: done.answered, cases: outcomes };
    yield* fs.writeFileString(`${target}${RESULTS}`, `${yield* Schema.encodeEffect(Results)(results)}\n`);
    yield* Console.log(`${chosen.name} ${version} ${done.host.type} ${done.host.variation} ${target}${RESULTS}`);
    yield* Effect.forEach(outcomes, (row) => Effect.flatMap(summary(row), Console.log), { discard: true });
    const instant = DateTime.formatIso(yield* DateTime.now);
    const kept = (row: Row, outcome: typeof Outcome.Type) => Schema.is(Probe)(row) || outcome.crashed !== undefined || outcome.hung !== undefined;
    const factOf = (row: Row) => (outcome: typeof Outcome.Type) =>
        Option.zipWith(
            Option.filter(answerOf(outcome, outcome.value ?? null), () => kept(row, outcome)),
            keyOf(row, Option.some(done.host)),
            (answer, key): typeof Facts.Fact.Type => ({ ...key, question: row.name, answer, asked: instant }),
        );
    const made = [
        ...(Option.isNone(known) ? [{ ...asked(MODEL), answer: { value: yield* Schema.encodeEffect(Model)(done.model) }, asked: instant }] : []),
        { ...asked(HOST), answer: { value: yield* Schema.encodeEffect(Host)(done.host) }, asked: instant },
        ...plan.rows.flatMap((row) =>
            Option.toArray(
                Option.flatMap(
                    Array.findFirst(outcomes, ({ name }) => name === row.name),
                    factOf(row),
                ),
            ),
        ),
    ];
    yield* Option.match(
        Option.filter(index, () => Array.isArrayNonEmpty(made)),
        { onNone: () => Effect.void, onSome: (held) => Facts.record(held, made) },
    );
    yield* plan.batch.module === undefined ? Effect.void : compared(root, plan.batch, plan.rows, file, outcomes);
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
        limit: Flag.String('limit').pipe(Flag.withSchema(Schema.DurationFromString), Flag.withDefault(Duration.minutes(2)), Flag.withDescription('Time one case may run, a batch allowed that per case, before it records hung')),
        facts: Flag.Path('facts').pipe(Flag.optional, Flag.withDescription('Facts index holding the object model, host state, evaluate answers, and crashed or hung cases, each new one appended')),
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
        const against = (outcome: typeof Results.Type) => Effect.andThen(Console.log(`${outcome.application} ${outcome.version}`), compared(root, batch, rows, file, outcome.cases));
        yield* Effect.forEach(found, (name) => Effect.flatMap(Effect.flatMap(fs.readFileString(path.join(path.dirname(file), name)), Schema.decodeEffect(Results)), against));
        yield* found.length === 0 ? HarnessError.make({ reason: { _tag: 'Unavailable', because: `no ${prefix}<version>${RESULTS} beside it` }, subject: file }) : Effect.void;
    }),
);

Command.make('acrobat').pipe(Command.withSubcommands([run, checked]), Command.run({ version: effect.version }), Effect.provide(layer), runMain);
