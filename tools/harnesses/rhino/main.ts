import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Array, Console, DateTime, Duration, Effect, FileSystem, Match, Option, Path, Record, Schema } from 'effect';
import { Argument, Command, Flag } from 'effect/cli';
import effect from 'effect/package.json' with { type: 'json' };
import { HarnessError } from '#/error';
import { facts, statements } from '#/facts';
import { Completion } from '#/results';
import { Batch, Engine, execute, type Hosted, Install, Mode, type Outcome, prepare, Results } from '#/rhino/host';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CASES = /\.cases$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Facts = facts(Engine);
const Report = Schema.fromJsonString(Schema.Unknown, { space: 2 });
const Line = Schema.fromJsonString(Schema.Unknown);
const Questions = Schema.fromJsonString(Batch);
const Interrupted = Schema.TemplateLiteral([Schema.Literals(['crashed', 'hung']), ' ', Schema.String]).annotate({ description: 'Thrown answer of a case that ended its instance' });
const same = Schema.toEquivalence(Schema.UndefinedOr(Schema.Json));

// --- [OPERATIONS] ----------------------------------------------------------------------

const stemOf = Effect.fn('stemOf')(function* (file: string, version: string) {
    const path = yield* Path.Path;
    return path.join(path.dirname(file), `${path.basename(file, '.json').replace(CASES, '')}.${version}`);
});
const answerOf = (row: typeof Outcome.Type): Option.Option<(typeof Facts.Fact.Type)['answer']> =>
    Option.firstSomeOf([
        Option.map(Option.fromUndefinedOr(row.crashed), (crashed) => ({ thrown: `crashed ${[crashed.exception, crashed.type].filter((part) => part !== undefined).join(' ')}` })),
        Option.map(Option.fromUndefinedOr(row.hung), ({ cpu }) => ({ thrown: `hung ${cpu}` })),
        Completion.match(row.completion, { Ran: () => Option.some({ value: row.value ?? null }), Thrown: ({ message }) => Option.some({ thrown: message }), Exited: () => Option.none(), Failed: () => Option.none() }),
    ]);
const interrupting = (answer: (typeof Facts.Fact.Type)['answer']) => 'thrown' in answer && Schema.is(Interrupted)(answer.thrown);
const keyOf = (install: Install, row: Hosted) => ({ question: row.name, identifier: install.bundle, version: install.version, engine: Engine.literal, expression: row.expression, precondition: row.precondition });
const summary = (row: typeof Outcome.Type) =>
    [
        row.name,
        Completion.match(row.completion, { Ran: () => 'ran', Exited: () => 'exited', Thrown: ({ message, file, line }) => `thrown ${message}${line === undefined ? '' : ` at ${file ?? ''}:${line}`}`, Failed: ({ reason }) => `failed ${reason}` }),
        ...(row.answered.length === 0 ? [] : [`answered ${row.answered.map(({ pressed, text }) => pressed ?? text.join(' ')).join(', ')}`]),
        ...(row.cancelled === undefined ? [] : [`cancelled ${row.cancelled.join(', ')}`]),
        ...(row.leaked.length === 0 ? [] : [`leaked ${row.leaked.join(', ')}`]),
    ].join(' | ');
const shown = (label: string, question: string, answer: unknown) => Effect.flatMap(Schema.encodeEffect(Line)(answer), (text) => Console.log(`${label}: ${question} | ${text}`));
const decoded = Effect.fn('decoded')(function* (file: string) {
    const fs = yield* FileSystem.FileSystem;
    return yield* Schema.decodeEffect(Questions)(yield* fs.readFileString(file), { onExcessProperty: 'error' });
});
const plugged = (root: string, batch: typeof Batch.Type) => Effect.map(Path.Path, (path) => Option.map(Option.fromUndefinedOr(batch.plugin), (file) => path.join(root, file)));
const asked = Effect.fn('asked')(function* (install: Install, root: string, file: string, batch: typeof Batch.Type, only: readonly string[], limit: Duration.Duration, index: Option.Option<string>, search: (question: string) => readonly string[]) {
    const fs = yield* FileSystem.FileSystem;
    const rows = yield* prepare(install, root, batch, only, Mode.Run());
    const indexed = yield* Option.match(index, { onNone: () => Effect.succeed([]), onSome: Facts.load });
    const known = (row: Hosted) => Facts.known(indexed, keyOf(install, row));
    const probing = (row: Hosted) => row.kind === 'Probe';
    const answered = rows.filter((row) => probing(row) && Option.isSome(known(row)));
    const interrupted = rows.flatMap((row) =>
        Option.toArray(
            Option.map(
                Option.filter(known(row), ({ answer }) => interrupting(answer)),
                (fact) => [row, fact] as const,
            ),
        ),
    );
    yield* Effect.forEach(Array.getSomes(answered.map(known)), ({ question, answer }) => shown('known', question, answer), { discard: true });
    yield* Effect.forEach(
        rows.filter((row) => probing(row) && Option.isNone(known(row))).flatMap(({ name }) => search(name).map((line) => [name, line] as const)),
        ([name, line]) => Console.log(`stated: ${name} | ${line}`),
        { discard: true },
    );
    const skipped = new Set([...answered, ...interrupted.map(([row]) => row)].map(({ name }) => name));
    const pending = rows.filter(({ name }) => !skipped.has(name));
    const outcome = yield* Array.match(pending, {
        onEmpty: () => Effect.succeed<typeof Results.Type>({ application: install.name, identifier: install.bundle, version: install.version, engine: Engine.literal, answered: [], cases: [] }),
        onNonEmpty: (held) => Effect.flatMap(plugged(root, batch), (plugin) => Effect.flatMap(stemOf(file, install.version), (stem) => execute(install, held, Mode.Run(), `${stem}.captures`, limit, plugin))),
    });
    const kept = interrupted.map(([row, fact]): typeof Outcome.Type => ({ name: row.name, completion: { _tag: 'Failed', reason: `known ${'thrown' in fact.answer ? fact.answer.thrown : ''} since ${fact.asked}` }, answered: [], leaked: [] }));
    const every: typeof Results.Type = { ...outcome, cases: Array.getSomes(rows.map(({ name }) => Array.findFirst([...outcome.cases, ...kept], (held) => held.name === name))) };
    const target = `${yield* stemOf(file, install.version)}.results.json`;
    yield* fs.writeFileString(target, `${yield* Schema.encodeEffect(Results)(every)}\n`);
    yield* Console.log([`${every.application} ${every.version} ${every.engine} ${target}`, ...Option.toArray(Option.fromUndefinedOr(every.failed))].join(' failed: '));
    yield* Effect.forEach(every.cases, (row) => Console.log(summary(row)), { discard: true });
    const at = DateTime.formatIso(yield* DateTime.now);
    const host = { language: every.language?.lcid ?? null, plugins: (every.plugins ?? []).map(({ name, version }) => `${name} ${version}`), grasshopper: every.grasshopper?.version ?? null };
    const factOf = (row: Hosted) => (held: typeof Outcome.Type) =>
        Option.map(
            Option.filter(answerOf(held), (answer) => probing(row) || interrupting(answer)),
            (answer): typeof Facts.Fact.Type => ({ ...keyOf(install, row), host, answer, asked: at }),
        );
    const made = Array.getSomes(
        pending.map((row) =>
            Option.flatMap(
                Array.findFirst(outcome.cases, ({ name }) => name === row.name),
                factOf(row),
            ),
        ),
    );
    yield* Option.match(
        Option.filter(index, () => made.length > 0),
        { onNone: () => Effect.void, onSome: (held) => Facts.record(held, made) },
    );
    yield* Effect.forEach(made, ({ question, answer }) => shown('asked', question, answer), { discard: true });
});
const observed = (row: Hosted, outcome: typeof Outcome.Type) =>
    Completion.match(outcome.completion, {
        Ran: () =>
            Option.some(
                Match.value(row.expect).pipe(
                    Match.when('value', (): Schema.Json => outcome.value ?? null),
                    Match.when('writes', (): Schema.Json => outcome.writes ?? []),
                    Match.orElse((): Schema.Json => Record.filter(outcome.read ?? {}, (_, key) => row.keys.length === 0 || row.keys.includes(key))),
                ),
            ),
        Thrown: ({ message }) => Option.some<Schema.Json>({ thrown: message }),
        Exited: () => Option.none(),
        Failed: () => Option.none(),
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

const limitFlag = Flag.String('limit').pipe(Flag.withSchema(Schema.DurationFromString), Flag.withDefault(Duration.minutes(2)), Flag.withDescription('Time the case journal stays unchanged before a waiting prompt is cancelled or the instance is sampled and ended'));
const product = Argument.Directory('product', { mustExist: true }).pipe(Argument.withDescription('Product folder holding the case paths, plug-in, and rebuilt sources'));
const casesFile = Argument.File('cases', { mustExist: true }).pipe(Argument.withDescription('Case file, results written beside it as <name>.<version>.results.json'));
const run = Command.make(
    'run',
    {
        product,
        cases: casesFile,
        host: Flag.String('host').pipe(Flag.atLeast(1), Flag.withDefault([]), Flag.withDescription('Bundle id, CFBundleName, bundle path, or version prefix of an install, repeated, every install when absent')),
        limit: limitFlag,
        facts: Flag.Path('facts').pipe(Flag.optional, Flag.withDescription('Facts index answering known probes and skipping known crashes before the host call, each new answer appended')),
        stated: Flag.File('stated', { mustExist: true }).pipe(Flag.atLeast(1), Flag.withDefault([]), Flag.withDescription('Text facts file whose lines matching a probe print beside it, repeated')),
        only: Flag.String('only').pipe(Flag.atLeast(1), Flag.withDefault([]), Flag.withDescription('Case name to run, repeated')),
    },
    ({ product: folder, cases, host: keys, limit, facts: index, stated, only }) =>
        Effect.gen(function* () {
            const path = yield* Path.Path;
            const [file, root] = [path.resolve(cases), path.resolve(folder)];
            const [batch, search, picked] = yield* Effect.all([decoded(file), statements(stated), Effect.flatMap(Install.all, (rows) => Install.chosen(rows, keys))]);
            yield* Effect.forEach(picked, (install) => asked(install, root, file, batch, only, limit, index, search), { discard: true });
        }),
);
const compared = Command.make('compare', { product, cases: casesFile, release: Flag.String('release').pipe(Flag.withDescription('Version a results file holds')), limit: limitFlag }, ({ product: folder, cases, release, limit }) =>
    Effect.gen(function* () {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const [file, root] = [path.resolve(cases), path.resolve(folder)];
        const stem = yield* stemOf(file, release);
        const recorded = yield* Effect.flatMap(fs.readFileString(`${stem}.results.json`), Schema.decodeEffect(Results));
        const install = yield* Install.reporting(yield* Install.all, recorded.identifier, recorded.version);
        const batch = yield* decoded(file);
        const module = yield* Effect.fromOption(
            Option.map(Option.fromUndefinedOr(batch.module), (held) => path.join(root, held)),
            () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'case file names no module' }, subject: cases }),
        );
        const rows = yield* prepare(install, root, batch, [], Mode.Compare({ module }));
        const rebuilt = yield* execute(install, rows, Mode.Compare({ module }), `${stem}.captures/rebuilt`, limit, yield* plugged(root, batch));
        const outcomes = (from: typeof Results.Type, row: Hosted) =>
            Option.flatMap(
                Array.findFirst(from.cases, ({ name }) => name === row.name),
                (held) => observed(row, held),
            );
        const verdicts = rows.map((row) => ({ name: row.name, expect: row.expect, host: outcomes(recorded, row), rebuilt: outcomes(rebuilt, row) }));
        const unproven = verdicts.filter(({ host, rebuilt: made }) => Option.isNone(host) || Option.isNone(made)).map(({ name }) => name);
        const mismatches = verdicts.filter(({ host, rebuilt: made }) => Option.isSome(host) && Option.isSome(made) && !same(host.value, made.value)).map(({ name, expect, host, rebuilt: made }) => ({ name, expect, host: Option.getOrNull(host), rebuilt: Option.getOrNull(made) }));
        yield* Console.log(yield* Schema.encodeEffect(Report)({ cases: verdicts.length, agreed: verdicts.length - mismatches.length - unproven.length, ...(rebuilt.failed === undefined ? {} : { failed: rebuilt.failed }), unproven, mismatches }));
        yield* mismatches.length === 0 ? Effect.void : HarnessError.make({ reason: { _tag: 'Disagreed', mismatches: mismatches.length }, subject: module });
    }),
);

Command.make('rhino').pipe(Command.withSubcommands([run, compared]), Command.run({ version: effect.version }), Effect.provide(layer), runMain);
