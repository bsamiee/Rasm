import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Array, Console, DateTime, Duration, Effect, FileSystem, Match, Option, Path, Record, Schema, Struct } from 'effect';
import { Argument, Command, Flag } from 'effect/cli';
import effect from 'effect/package.json' with { type: 'json' };
import { type Batch, Engine, HostCase, type Outcome, Results } from '#/blender/contract';
import { execute, installations, prepare } from '#/blender/host';
import { HarnessError } from '#/error';
import { answerOf, facts, statements } from '#/facts';
import { Completion } from '#/results';

// --- [TYPES] ---------------------------------------------------------------------------

type Row = (typeof Batch.Type.cases)[number];
type Prepared = Effect.Success<ReturnType<typeof prepare>>;
type Item = Prepared['items'][number];
type Install = Effect.Success<ReturnType<typeof installations>>[number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const RESULTS = '.results.json';
const DICTIONARY = { question: 'RNA dictionary of every bpy.types struct and bpy.ops operator', expression: 'bpy.types, bpy.ops' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Facts = facts(Engine);
const Line = Schema.fromJsonString(Schema.Unknown);
const Report = Schema.fromJsonString(Schema.Unknown, { space: 2 });
const same = Schema.toEquivalence(Schema.Json);

// --- [OPERATIONS] ----------------------------------------------------------------------

const stem = (file: string) => Effect.map(Path.Path, (path) => path.join(path.dirname(file), path.basename(file, '.cases.json')));
const calledOf = (row: Row) =>
    Match.value(row).pipe(
        Match.when({ fn: Match.string }, ({ fn, args, keywords }) => ({ fn, args: args ?? [], keywords: keywords ?? {} })),
        Match.option,
    );
const observed = (row: typeof Outcome.Type, expect: NonNullable<typeof Batch.Type.expect>, keys: readonly string[]): Schema.Json => ({
    completion: Completion.match(row.completion, { Ran: () => 'Ran', Exited: () => 'Exited', Thrown: ({ message }) => message, Failed: () => 'Failed' }),
    ...Match.value(expect).pipe(
        Match.when('value', () => ({ value: row.value ?? null })),
        Match.when('read', () => ({ read: Record.filter(row.read ?? {}, (_, key) => keys.length === 0 || keys.includes(key)) })),
        Match.when('writes', () => ({ writes: row.writes ?? null })),
        Match.exhaustive,
    ),
    files: Record.map(row.files, ({ sha256, pixels }) => pixels ?? sha256),
    modal: row.modal,
    windows: row.windows,
    interrupted: row.crashed !== undefined || row.hung !== undefined,
});
const summary = Effect.fn('summary')(function* (row: typeof Outcome.Type) {
    const shown = { ...Record.filter({ answered: row.answered, leaked: row.leaked, modal: row.modal, jobs: row.jobs }, (items) => items.length > 0), ...(row.crashed === undefined ? {} : { crashed: row.crashed }), ...(row.hung === undefined ? {} : { hung: row.hung }) };
    const completion = Completion.match(row.completion, {
        Ran: () => 'ran',
        Exited: () => 'exited',
        Thrown: ({ message, file, line }) => `thrown ${message}${line === undefined ? '' : ` at ${file ?? ''}:${line}`}`,
        Failed: ({ reason }) => `failed ${reason}`,
    });
    return [row.name, completion, ...(Struct.keys(shown).length === 0 ? [] : [yield* Schema.encodeEffect(Line)(shown)])].join(' | ');
});
const compared = Effect.fn('compared')(function* (install: Install, root: string, file: string, prepared: Prepared, outcomes: readonly (typeof Outcome.Type)[], limit: Duration.Duration) {
    const path = yield* Path.Path;
    const { batch, items } = prepared;
    const module = yield* Effect.fromOption(Option.fromUndefinedOr(batch.module), () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'case file names no module' }, subject: file }));
    const pairs = Array.getSomes(
        items.map(({ row, host }) =>
            Option.map(Option.all({ called: calledOf(row), outcome: Array.findFirst(outcomes, ({ name }) => name === host.name) }), ({ called, outcome }) => ({
                outcome,
                expect: row.expect ?? batch.expect ?? (HostCase.guards.Call(host) ? 'value' : 'read'),
                keys: row.keys ?? batch.keys ?? [],
                rebuilt: { host: HostCase.cases.Call.make({ ...Struct.pick(host, ['name', 'precondition', 'read', 'confirms', 'files']), script: path.join(root, module), call: called.fn, args: called.args, keywords: called.keywords }), presses: [] },
            })),
        ),
    );
    const captures = `${yield* stem(file)}.${install.version}.rebuilt.captures`;
    const rebuilt = yield* Array.match(
        pairs.map(({ rebuilt: call }) => call),
        { onEmpty: () => Effect.succeed([]), onNonEmpty: (calls) => Effect.map(execute(install, calls, limit, captures, Option.none()), ({ cases }) => cases) },
    );
    const mismatches = pairs.flatMap(({ outcome, expect, keys }) => {
        const left = observed(outcome, expect, keys);
        return Option.match(
            Option.map(
                Array.findFirst(rebuilt, ({ name }) => name === outcome.name),
                (row) => observed(row, expect, keys),
            ),
            { onNone: () => [{ name: outcome.name, host: left }], onSome: (right) => (same(left, right) ? [] : [{ name: outcome.name, host: left, rebuilt: right }]) },
        );
    });
    yield* Console.log(yield* Schema.encodeEffect(Report)({ cases: pairs.length, agreed: pairs.length - mismatches.length, mismatches }));
    yield* mismatches.length === 0 ? Effect.void : HarnessError.make({ reason: { _tag: 'Disagreed', mismatches: mismatches.length }, subject: module });
});
const released = Effect.fn('released')(function* (install: Install, root: string, file: string, only: readonly string[], limit: Duration.Duration, index: string, search: (question: string) => readonly string[]) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const prepared = yield* prepare(root, file, only);
    const indexed = yield* Facts.load(index);
    const installed = { identifier: install.identifier, version: install.version, engine: Engine.literal };
    const keyOf = ({ key }: Item) => ({ ...installed, ...key });
    const answered = (item: Item) => Option.orElse(HostCase.guards.Probe(item.host) ? Facts.known(indexed, { ...keyOf(item), question: item.host.name }) : Option.none(), () => Facts.faulted(indexed, keyOf(item)));
    const skipped = prepared.items.flatMap((item) => Option.toArray(Option.map(answered(item), ({ answer }) => ({ item, answer }))));
    const pending = prepared.items.filter((item) => !skipped.some((held) => held.item === item));
    yield* Effect.forEach(skipped, ({ item, answer }) => Effect.flatMap(Schema.encodeEffect(Line)(answer), (text) => Console.log(`known: ${item.host.name} | ${text}`)), { discard: true });
    yield* Effect.forEach(
        pending.filter(({ host }) => HostCase.guards.Probe(host)).flatMap(({ host }) => search(host.name).map((line) => `stated: ${host.name} | ${line}`)),
        Console.log,
        { discard: true },
    );
    const dictionary = path.join(path.dirname(index), `${path.basename(index, '.json')}.${install.version}.dictionary.json`);
    const asked = Option.liftPredicate(dictionary, () => Option.isNone(Facts.known(indexed, { ...installed, question: DICTIONARY.question })));
    const target = `${yield* stem(file)}.${install.version}`;
    const ran = yield* pending.length === 0 && Option.isNone(asked) ? Effect.succeedNone : Effect.asSome(execute(install, pending, limit, `${target}.captures`, asked));
    yield* Option.match(
        Option.filter(ran, () => pending.length > 0),
        {
            onNone: () => Effect.void,
            onSome: (results) =>
                Effect.gen(function* () {
                    yield* fs.writeFileString(`${target}${RESULTS}`, `${yield* Schema.encodeEffect(Results)(results)}\n`);
                    yield* Console.log(`${results.application} ${results.version} ${target}${RESULTS}`);
                    yield* Effect.forEach(results.cases, (row) => Effect.flatMap(summary(row), Console.log), { discard: true });
                }),
        },
    );
    const instant = DateTime.formatIso(yield* DateTime.now);
    const outcomes = Option.match(ran, { onNone: () => [], onSome: ({ cases }) => cases });
    const hostOf = Option.match(
        Option.flatMap(ran, ({ host }) => Option.fromUndefinedOr(host)),
        { onNone: () => ({}), onSome: (host) => ({ host }) },
    );
    const factOf = (item: Item) =>
        Option.flatMap(
            Array.findFirst(outcomes, ({ name }) => name === item.host.name),
            (outcome) =>
                Option.map(
                    Option.filter(answerOf(outcome, outcome.value ?? null), () => HostCase.guards.Probe(item.host) || outcome.crashed !== undefined || outcome.hung !== undefined),
                    (answer): typeof Facts.Fact.Type => ({ ...keyOf(item), question: item.host.name, ...hostOf, answer, asked: instant }),
                ),
        );
    const described = yield* Option.match(asked, {
        onNone: () => Effect.succeed([]),
        onSome: (held) =>
            Effect.flatMap(fs.exists(held), (found) =>
                found ? Effect.as(Console.log(`dictionary ${held}`), [{ ...installed, ...DICTIONARY, ...hostOf, answer: { value: path.basename(held) }, asked: instant } satisfies typeof Facts.Fact.Type]) : HarnessError.make({ reason: { _tag: 'Unavailable', because: 'wrote no RNA dictionary' }, subject: held }),
            ),
    });
    const made = [...Array.getSomes(prepared.items.map(factOf)), ...described];
    yield* Array.isArrayNonEmpty(made) ? Facts.record(index, made) : Effect.void;
    yield* prepared.batch.module === undefined || pending.length === 0 ? Effect.void : compared(install, root, file, prepared, outcomes, limit);
});
const recompared = Effect.fn('recompared')(function* (root: string, file: string, prepared: Prepared, results: string, limit: Duration.Duration) {
    const fs = yield* FileSystem.FileSystem;
    const recorded = yield* Effect.flatMap(fs.readFileString(results), Schema.decodeEffect(Results));
    const install = yield* Effect.fromOption(
        Array.findFirst(yield* installations([recorded.version]), ({ version }) => version === recorded.version),
        () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'is not installed' }, subject: `Blender ${recorded.version}` }),
    );
    yield* Console.log(`${recorded.application} ${recorded.version}`);
    yield* compared(install, root, file, prepared, recorded.cases, limit);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const product = Argument.Directory('product', { mustExist: true }).pipe(Argument.withDescription('Product folder holding the scripts, documents, and rebuilt modules'));
const casesFile = Argument.File('cases', { mustExist: true }).pipe(Argument.withDescription('Case file, results written beside it as <name>.<version>.results.json'));
const limited = Flag.String('limit').pipe(Flag.withSchema(Schema.DurationFromString), Flag.withDefault(Duration.fromInputUnsafe('5 minutes')), Flag.withDescription('Time a case settles, and an instance reads unresponsive before it is sampled and ended'));
const run = Command.make(
    'run',
    {
        product,
        cases: casesFile,
        release: Flag.String('release').pipe(Flag.atLeast(1), Flag.withDefault([]), Flag.withDescription('Version prefix of <release>[-<cycle>]+<hash> the install reports, repeated, every install when absent')),
        only: Flag.String('only').pipe(Flag.atLeast(1), Flag.withDefault([]), Flag.withDescription('Case name to run, repeated')),
        limit: limited,
        facts: Flag.Path('facts').pipe(Flag.withDescription('Facts index answering known evaluate cases and skipping crashed or hung ones before the host call, each new one appended, the RNA dictionary of each version written beside it')),
        stated: Flag.File('stated', { mustExist: true }).pipe(Flag.atLeast(1), Flag.withDefault([]), Flag.withDescription('Text facts file whose lines matching a question print beside it, repeated')),
    },
    ({ product: folder, cases, release, only, limit, facts: index, stated }) =>
        Effect.gen(function* () {
            const path = yield* Path.Path;
            const search = yield* statements(stated);
            yield* Effect.forEach(yield* installations(release), (install) => released(install, path.resolve(folder), path.resolve(cases), only, limit, path.resolve(index), search), { discard: true });
        }),
);
const checked = Command.make('compare', { product, cases: casesFile, limit: limited }, ({ product: folder, cases, limit }) =>
    Effect.gen(function* () {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const [root, file] = [path.resolve(folder), path.resolve(cases)];
        const prepared = yield* prepare(root, file, []);
        const prefix = `${path.basename(yield* stem(file))}.`;
        const found = (yield* fs.readDirectory(path.dirname(file))).filter((name) => name.startsWith(prefix) && name.endsWith(RESULTS));
        yield* found.length === 0 ? HarnessError.make({ reason: { _tag: 'Unavailable', because: `no ${prefix}<version>${RESULTS} beside it` }, subject: file }) : Effect.void;
        yield* Effect.forEach(found, (name) => recompared(root, file, prepared, path.join(path.dirname(file), name), limit), { discard: true });
    }),
);

Command.make('blender').pipe(Command.withSubcommands([run, checked]), Command.run({ version: effect.version }), Effect.provide(layer), runMain);
