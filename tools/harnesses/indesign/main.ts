import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Array, Console, DateTime, Effect, FileSystem, Option, Path, Schema } from 'effect';
import { Argument, Command, Flag } from 'effect/cli';
import effect from 'effect/package.json' with { type: 'json' };
import { compare } from '#/compare';
import { HarnessError } from '#/error';
import { facts } from '#/facts';
import { APPLICATIONS, Engine, execute, expand, Host, normalize, Results, version } from '#/indesign/host';
import { Precondition } from '#/indesign/precondition';
import type { Outcome } from '#/results';
import { revive } from '#/values';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CASES_FILE = /\.cases\.json$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Facts = facts(Engine);
const Report = Schema.fromJsonString(Schema.Unknown, { space: 2 });
const Line = Schema.fromJsonString(Facts.Fact);
const Alerts = Schema.fromJsonString(Schema.Array(Schema.String));

// --- [OPERATIONS] ----------------------------------------------------------------------

const resultsOf = (file: string) => `${file.replace(CASES_FILE, '')}.results.json`;
const refusal = (row: typeof Outcome.Type) => Option.orElse(Option.fromUndefinedOr(row.thrown), () => Option.fromUndefinedOr(row.failed));
const summary = Effect.fn('summary')(function* (row: typeof Outcome.Type) {
    const alerts = row.alerts.length > 0 ? [`alerts ${yield* Schema.encodeEffect(Alerts)(row.alerts)}`] : [];
    return [row.name, row.ran ? 'ran' : 'not run', ...Option.toArray(refusal(row)), ...alerts].join(' | ');
});
const asked = Effect.fn('asked')(function* (host: typeof Host.Type, engine: typeof Engine.Type, key: { readonly question: string; readonly application: string }, expression: string, precondition: Option.Option<typeof Precondition.Type>) {
    const path = yield* Path.Path;
    const given = Option.getOrElse(precondition, () => ({}));
    const outcome = yield* execute(host, engine, path.resolve('.'), [{ name: key.question, kind: 'probe', evaluate: expression, args: [], precondition: given, read: {} }]);
    const row = yield* Effect.fromOption(Array.head(outcome.cases), () => HarnessError.make({ reason: { _tag: 'Unavailable', because: outcome.failed ?? 'host returned no case' }, subject: key.question }));
    const answer = Option.match(refusal(row), { onNone: () => ({ value: row.value ?? null }), onSome: (thrown) => ({ thrown }) });
    return { ...key, version: outcome.version, engine, expression, ...(Option.isSome(precondition) ? { precondition: given } : {}), answer, asked: DateTime.formatIso(yield* DateTime.now) };
});
const paired = (outcomes: readonly (typeof Outcome.Type)[]) => (row: Effect.Success<ReturnType<typeof expand>>['rows'][number]) =>
    Option.flatMap(Option.fromUndefinedOr('fn' in row ? row.fn : undefined), (fn) =>
        Option.map(
            Array.findFirst(outcomes, ({ name }) => name === row.name),
            (found) => ({ name: row.name, fn, args: 'args' in row ? (row.args ?? []).map(revive) : [], host: Option.match(refusal(found), { onSome: (thrown) => ({ thrown }), onNone: () => ({ value: revive(('call' in row ? found.value : found.read) ?? null) }) }) }),
        ),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

const hostFlag = Flag.Literals('host', Host.literals).pipe(Flag.withDefault('2027' as const), Flag.withDescription('InDesign release, 2026 where the 2027 Beta refuses'));
const product = Argument.Directory('product', { mustExist: true }).pipe(Argument.withDescription('Product folder holding the scripts and rebuilt modules'));
const casesFile = Argument.File('cases', { mustExist: true }).pipe(Argument.withDescription('Case file, results written beside it as <name>.results.json'));
const run = Command.make('run', { product, cases: casesFile, host: hostFlag, only: Flag.String('only').pipe(Flag.atLeast(1), Flag.withDefault([]), Flag.withDescription('Case name to run, repeated')) }, ({ product: folder, cases, host, only }) =>
    Effect.gen(function* () {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const root = path.resolve(folder);
        const { engine, hosted } = yield* normalize(root, path.resolve(cases), only);
        const outcome = yield* execute(host, engine, root, hosted);
        yield* fs.writeFileString(resultsOf(path.resolve(cases)), `${yield* Schema.encodeEffect(Results)(outcome)}\n`);
        yield* Console.log([`${outcome.application} ${outcome.version} ${outcome.engine}`, ...Option.toArray(Option.fromUndefinedOr(outcome.failed))].join(' failed: '));
        yield* Effect.forEach(outcome.cases, (row) => Effect.flatMap(summary(row), Console.log), { discard: true });
    }),
);
const probe = Command.make(
    'probe',
    {
        question: Argument.String('question').pipe(Argument.withDescription('Host behavior asked, the facts index key')),
        expression: Argument.String('expression').pipe(Argument.withDescription('Host expression reading doc, app, and made')),
        facts: Flag.Path('facts').pipe(Flag.withDescription('Facts index file, checked before any host call')),
        host: hostFlag,
        engine: Flag.Literals('engine', Engine.literals).pipe(Flag.withDefault('extendscript' as const)),
        precondition: Flag.String('precondition').pipe(Flag.withSchema(Schema.fromJsonString(Precondition)), Flag.optional, Flag.withDescription('Precondition JSON for the probe document')),
    },
    ({ question, expression, facts: index, host, engine, precondition }) =>
        Effect.gen(function* () {
            const key = { question, application: APPLICATIONS[host], version: yield* version(host), engine };
            const known = Facts.known(yield* Facts.load(index), key);
            const fact = yield* Option.match(known, { onSome: Effect.succeed, onNone: () => Effect.tap(asked(host, engine, key, expression, precondition), (made) => Facts.record(index, made)) });
            yield* Console.log(`${Option.isSome(known) ? 'known' : 'asked'}: ${yield* Schema.encodeEffect(Line)(fact)}`);
        }),
);
const compared = Command.make('compare', { product, cases: casesFile }, ({ product: folder, cases }) =>
    Effect.gen(function* () {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const root = path.resolve(folder);
        const { batch, rows } = yield* expand(root, path.resolve(cases));
        const outcome = yield* Effect.flatMap(fs.readFileString(resultsOf(path.resolve(cases))), Schema.decodeEffect(Results));
        const module = yield* Effect.fromOption(Option.fromUndefinedOr(batch.module), () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'case file names no module' }, subject: cases }));
        const report = yield* compare(path.join(root, module), Array.getSomes(rows.map(paired(outcome.cases))));
        yield* Console.log(yield* Schema.encodeEffect(Report)(report));
        yield* report.mismatches.length === 0 ? Effect.void : HarnessError.make({ reason: { _tag: 'Disagreed', mismatches: report.mismatches.length }, subject: module });
    }),
);

Command.make('indesign').pipe(Command.withSubcommands([run, probe, compared]), Command.run({ version: effect.version }), Effect.provide(layer), runMain);
