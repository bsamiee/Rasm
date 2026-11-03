import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Automation } from '@rasm/automation/automation';
import { Array, Config, Console, DateTime, Duration, Effect, FileSystem, Layer, Match, Number, Option, Order, Path, Predicate, Record, Result, Schema, Struct, UndefinedOr } from 'effect';
import { Argument, Command, Flag } from 'effect/cli';
import effect from 'effect/package.json' with { type: 'json' };
import { answers, type Batch, Call, expand, Probe, type Row, scriptOf } from '#/cases';
import { compare, compared, type Pair, type Reply } from '#/compare';
import { HarnessError } from '#/error';
import { answerOf, facts, statements } from '#/facts';
import { merged } from '#/precondition';
import { type Answer, Completion, Outcome, results } from '#/results';
import { type Cased, type Decoding, execute, type Fields, type Host, type Installed, installations, type Plan, type Prepared, type Rebuilt, rerun, type Session, type Surveyed } from '#/session';
import { prepared } from '#/source';
import { bury, JsonObject, revive } from '#/values';

// --- [TYPES] ---------------------------------------------------------------------------

type Facts = ReturnType<typeof facts>;
type Fact = Facts['Fact']['Type'];
type Asking<T> = Pick<Fact, 'identifier' | 'version'> & { readonly engine: T };
type Item<C extends Cased> = Prepared<C> & { readonly row: Row };
interface Planned<C extends Cased> extends Plan<C, unknown> {
    readonly sendable: readonly Item<C>[];
    readonly skipped: readonly { readonly item: Item<C>; readonly answer: typeof Answer.Type }[];
}
interface Evidence {
    readonly answer: typeof Answer.Type;
    readonly before: typeof JsonObject.Type;
    readonly read: typeof JsonObject.Type;
}
interface Hosted<C extends Cased> {
    readonly chosen: Installed;
    readonly items: readonly Item<C>[];
    readonly engine: unknown;
}
interface Verdict {
    readonly mismatches: number;
    readonly failed: Option.Option<string>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const CASES = '.cases.json';
const RESULTS = '.results.json';

// --- [MODELS] --------------------------------------------------------------------------

const Line = Schema.fromJsonString(Schema.Unknown);
const atelier = Automation.layer(Config.String('ATELIER_PATH'));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [PATHS]
const stem = (path: Path.Path, file: string) => path.join(path.dirname(file), path.basename(file, CASES));
const ancestors = (path: Path.Path, folder: string): readonly string[] => (path.dirname(folder) === folder ? [folder] : [folder, ...ancestors(path, path.dirname(folder))]);
const casesUnder = Effect.fn('casesUnder')(function* (target: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { type } = yield* fs.stat(target);
    const found = type === 'Directory' ? (yield* fs.glob(`**/*${CASES}`, { root: target })).map((file) => path.join(target, file)) : [target];
    return found.map((file) => path.resolve(file));
});
const targets = (paths: readonly string[]) => Effect.map(Effect.forEach(paths, casesUnder), (files) => Array.dedupe(files.flat()).toSorted());
const loaded = Effect.fn('loaded')(function* (name: string, host: Pick<Host, 'Engine'>, file: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const proof = 'proof';
    const product = path.dirname(
        Option.getOrElse(
            Array.findFirst(ancestors(path, path.dirname(file)), (held) => path.basename(held) === proof),
            () => file,
        ),
    );
    const segments = product.split(path.sep);
    const folder = Option.map(
        Array.findFirstIndex(segments, (segment, at) => segment === 'applications' && segments[at + 1] === name),
        (at) => path.join(segments.slice(0, at + 2).join(path.sep), 'facts'),
    );
    const index = Option.map(folder, (held) => path.join(held, `${name}-host.json`));
    const Facts = facts(host.Engine);
    const indexed = yield* Option.match(index, { onNone: () => Effect.succeed([]), onSome: Facts.load });
    const stated = yield* Option.match(folder, { onNone: () => Effect.succeed([]), onSome: (held) => Effect.map(fs.glob('*.txt', { root: held }), (files) => files.map((entry) => path.join(held, entry))) });
    return { product, Facts, index, indexed, stated, ...(yield* expand(product, file)) };
});
const surveyOf = <S extends Surveyed>(host: Pick<Host<S>, 'survey'>, Facts: Facts, indexed: readonly Fact[], asking: Asking<unknown>) =>
    Option.flatMap(Facts.known(indexed, { ...asking, question: host.survey.question, expression: host.survey.question }), ({ answer }) => ('value' in answer ? Schema.decodeOption(host.survey.Schema)(answer.value) : Option.none()));

// --- [PLANNING]
const expressionOf = (batch: typeof Batch.Type, row: Row) =>
    Match.value(row).pipe(
        Match.when(Schema.is(Probe), ({ evaluate }) => Option.some(evaluate)),
        Match.when(Schema.is(Call), ({ call }) => Option.some(call)),
        Match.orElse(() => scriptOf(batch, row)),
    );
const argsOf = (row: Row) => (Schema.is(Probe)(row) ? [] : (row.args ?? []));
const authored = (batch: typeof Batch.Type, row: Row) => merged(batch.precondition ?? {}, row.precondition ?? {});
const windowedOf = (host: Pick<Host, 'windowed'>, batch: typeof Batch.Type) => (row: Row) => host.windowed?.(authored(batch, row), row) ?? true;
const prepare = <C extends Cased, E, P>(host: Pick<Host<Surveyed, C, E, P>, 'placed' | 'Case' | 'refusal' | 'windowed'>, product: string, captures: string, batch: typeof Batch.Type, Precondition: Decoding<P>, engine: E) =>
    Effect.fn('prepare')(function* (row: Row, index: number) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const folder = path.join(captures, String(index));
        yield* fs.makeDirectory(folder, { recursive: true });
        const decoded = yield* Schema.decodeEffect(Precondition)(authored(batch, row));
        const { precondition, copies } = yield* host.placed(decoded, folder, product);
        const keyworded = UndefinedOr.match({ onUndefined: () => ({}), onDefined: (held: NonNullable<typeof Call.Type.keywords>) => ({ keywords: held }) });
        const located = (file: string | null) => Option.getOrNull(Option.map(Option.fromNullOr(file), (held) => path.join(product, held)));
        const kind = Match.value(row).pipe(
            Match.when(Schema.is(Probe), ({ evaluate }) => ({ _tag: 'Probe' as const, evaluate })),
            Match.when(Schema.is(Call), ({ call, args, keywords }) => ({ _tag: 'Call' as const, call, args: args.map((arg) => bury(arg)), ...keyworded(keywords) })),
            Match.orElse(({ args = [], keywords }) => ({ _tag: 'Run' as const, args: args.map((arg) => bury(arg)), ...keyworded(keywords) })),
        );
        const lines = Option.flatMap(Option.liftPredicate(row, Schema.is(Call)), ({ lines: held }) => Option.fromUndefinedOr(held));
        const scripted = yield* Effect.transposeOption(Option.map(scriptOf(batch, row), (script) => prepared(path.join(product, script), lines)));
        const given = {
            ...Record.filter(
                Record.fromIterableWith(Struct.keys(answers), (key) => [key, row[key]] as const),
                Predicate.isNotUndefined,
            ),
            ...Record.filter(
                {
                    commands: row.commands,
                    files: UndefinedOr.map(row.files, (files) => files.map(located)),
                    plugin: UndefinedOr.map(batch.plugin, (plugin) => path.join(product, plugin)),
                    source: Option.getOrUndefined(Option.map(scripted, ({ source }) => source)),
                },
                Predicate.isNotUndefined,
            ),
        };
        const hosted = yield* Schema.decodeEffect(host.Case)({ ...kind, name: row.name, precondition, read: { ...batch.read, ...row.read }, folder, interact: (row.presses ?? []).length > 0, ...given });
        const refused =
            host.refusal?.(
                engine,
                hosted,
                Option.exists(scripted, ({ binary }) => binary),
            ) ?? Option.none();
        return yield* Option.match(refused, {
            onNone: () => Effect.succeed<Item<C>>({ hosted, row, map: Option.match(scripted, { onNone: () => [], onSome: ({ map }) => map }), presses: row.presses ?? [], copies, windowed: windowedOf(host, batch)(row) }),
            onSome: HarnessError.unavailable(row.name),
        });
    });
const planning = <S extends Surveyed, C extends Cased, E, P>(host: Pick<Host<S, C, E, P>, 'precondition' | 'placed' | 'Case' | 'refusal' | 'windowed'>, Facts: Facts, indexed: readonly Fact[], asking: Asking<E>, product: string, captures: string, batch: typeof Batch.Type, rows: readonly Row[]) => {
    const failed = (row: Row) => (error: HarnessError | Schema.SchemaError) => Effect.map(Effect.orDie(Schema.decodeEffect(Outcome)({ name: row.name, completion: { _tag: 'Failed', reason: error.message } })), Result.fail);
    const keyOf = (survey: S) => (row: Row) => Option.map(expressionOf(batch, row), (expression) => ({ ...asking, question: row.name, expression, args: argsOf(row), precondition: authored(batch, row), host: survey.host }));
    const planned = Effect.fnUntraced(function* (survey: S) {
        const fs = yield* FileSystem.FileSystem;
        yield* fs.remove(captures, { recursive: true, force: true });
        const placing = prepare(host, product, captures, batch, host.precondition(survey), asking.engine);
        const made = yield* Effect.forEach(rows, (row, index) => placing(row, index).pipe(Effect.map(Result.succeed), Effect.catchTags({ HarnessError: failed(row), SchemaError: failed(row) })));
        const answered = (row: Row) => Option.flatMap(keyOf(survey)(row), (key) => Option.orElse(Facts.known(indexed, key), () => Facts.faulted(indexed, key)));
        const sendable = Array.getSuccesses(made);
        const skipped = Array.getSomes(sendable.map((item) => Option.map(answered(item.row), ({ answer }) => ({ item, answer }))));
        return { engine: asking.engine, sendable: sendable.filter((item) => !skipped.some((held) => held.item === item)), settled: Array.getFailures(made), skipped } satisfies Planned<C>;
    });
    return { planned, keyOf, windowed: Option.exists(Array.head(rows), windowedOf(host, batch)) };
};

// --- [COMPARING]
const revived = Record.map((held: Schema.Json) => revive(held));
const evidenceOf = (row: typeof Outcome.Type): Option.Option<Evidence> => Option.map(answerOf(row), (answer) => ({ answer, before: row.before ?? {}, read: row.read ?? {} }));
const paired = (batch: typeof Batch.Type, row: Row, fn: string, evidence: Evidence): Pair => {
    const expect = row.expect ?? batch.expect ?? (Schema.is(Call)(row) || Schema.is(Probe)(row) ? 'value' : 'read');
    const keys = row.keys ?? batch.keys ?? [];
    const picked = Record.filter((_: unknown, key: string) => keys.length === 0 || keys.includes(key));
    const before = revived(evidence.before);
    const read = revived(evidence.read);
    return {
        name: row.name,
        fn,
        args: argsOf(row).map((arg) => revive(arg, { before: (key) => before[key], read: (key) => read[key] })),
        host: 'thrown' in evidence.answer ? evidence.answer : { value: expect === 'value' ? revive(evidence.answer.value) : picked(read) },
        settled: (rebuilt) => (expect === 'writes' && Schema.is(JsonObject)(rebuilt) ? picked({ ...before, ...rebuilt }) : rebuilt),
    };
};
const pairsOf = (batch: typeof Batch.Type, rows: readonly Row[], evidence: (row: Row) => Option.Option<Evidence>) => {
    const proven = Array.getSomes(rows.map((row) => Option.map(Option.fromUndefinedOr(Schema.is(Probe)(row) ? undefined : row.fn), (fn) => ({ row, fn }))));
    return {
        pairs: Array.getSomes(proven.map(({ row, fn }) => Option.map(evidence(row), (held) => paired(batch, row, fn, held)))),
        unproven: proven.filter(({ row }) => Option.isNone(evidence(row))).map(({ row }) => row.name),
    };
};
const callOf =
    <C extends Cased>(items: readonly Item<C>[], module: string) =>
    (pair: Pair) =>
        Option.map(
            Array.findFirst(items, ({ hosted }) => hosted.name === pair.name),
            ({ hosted, row, windowed }): Rebuilt<C> => ({ hosted, windowed, fn: pair.fn, args: argsOf(row), module }),
        );
const replyOf = (row: typeof Outcome.Type) => Option.map(answerOf(row), (answer): readonly [string, Reply] => [row.name, 'value' in answer ? Result.succeed(revive(answer.value)) : Result.fail(answer.thrown)]);
const comparing = <S extends Surveyed, C extends Cased, X, R>(host: Session<S, C> & Pick<Host<S, C>, 'rebuilt'>, hosted: Effect.Effect<Hosted<C>, X, R>, module: string, pairs: readonly Pair[], unproven: readonly string[], scratch: string, limit: Duration.Duration) => {
    const { rebuilt } = host;
    return rebuilt === undefined
        ? compare(module, pairs, unproven)
        : Effect.gen(function* () {
              const { chosen, items, engine } = yield* hosted;
              const outcomes = yield* rerun(host, rebuilt, chosen, scratch, limit, engine, Array.getSomes(pairs.map(callOf(items, module))));
              return compared(module, pairs, new Map(Array.getSomes(outcomes.map(replyOf))), unproven);
          });
};

// --- [REPORTING]
const order = (rows: readonly Row[]) => Order.mapInput(Number.Order, ({ name }: typeof Outcome.Type) => rows.findIndex((row) => row.name === name));
const summary = (host: Pick<Host, 'shown'>) =>
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
const proved = Effect.fn('proved')(function* <S extends Surveyed, C extends Cased, E, P, F extends Fields>(name: string, host: Host<S, C, E, P, F>, chosen: Installed, file: string, only: readonly string[], limit: Duration.Duration) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { product, Facts, index, indexed, stated, batch, rows: every } = yield* loaded(name, host, file);
    const Results = results(host.Engine);
    const search = yield* statements(stated);
    yield* Schema.decodeEffect(Schema.Array(Schema.Literals(every.map((row) => row.name))))(only);
    const rows = every.filter((row) => only.length === 0 || only.includes(row.name));
    const engine = yield* Effect.fromResult(host.engine(Array.dedupe(Array.getSomes(rows.map((row) => scriptOf(batch, row))))));
    const asking: Asking<E> = { identifier: chosen.identifier, version: chosen.version, engine };
    const known = surveyOf(host, Facts, indexed, asking);
    const target = `${stem(path, file)}.${chosen.version}`;
    const scratch = yield* fs.makeTempDirectoryScoped({ prefix: `${name}-harness-` });
    const { planned, keyOf, windowed } = planning(host, Facts, indexed, asking, product, `${target}.captures`, batch, rows);
    const done = yield* execute(host, chosen, scratch, limit, windowed, known, planned);
    const { plan } = done;
    yield* Effect.forEach(plan.skipped, ({ item, answer }) => Effect.flatMap(Schema.encodeEffect(Line)(answer), (text) => Console.log(`known: ${item.hosted.name} | ${text}`)), { discard: true });
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
                Array.findFirst(outcomes, (held) => held.name === row.name),
                evidenceOf,
            ),
            () =>
                Option.map(
                    Array.findFirst(plan.skipped, ({ item }) => item.hosted.name === row.name),
                    ({ answer }): Evidence => ({ answer, before: {}, read: {} }),
                ),
        );
    const { pairs, unproven } = pairsOf(batch, rows, evidence);
    const section = yield* UndefinedOr.match(batch.module, {
        onUndefined: () => Effect.succeedNone,
        onDefined: (module) => Effect.asSome(comparing(host, Effect.succeed({ chosen, items: [...plan.sendable, ...plan.skipped.map(({ item }) => item)], engine }), path.join(product, module), pairs, unproven, scratch, limit)),
    });
    const header = Option.getOrUndefined(done.header);
    const written = {
        application: header?.application ?? chosen.name,
        identifier: chosen.identifier,
        version: chosen.version,
        engine,
        ...(header?.state === undefined ? {} : { state: header.state }),
        host: done.survey.host,
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
            Array.findFirst(outcomes, (held) => held.name === row.name),
            (held) => Option.zipWith(answerOf(held), keyOf(done.survey)(row), (answer, key): Fact => ({ ...key, answer, asked })),
        );
    const surveyed: readonly Fact[] = Option.isSome(done.header) && Option.isNone(known) ? [{ ...asking, question: host.survey.question, expression: host.survey.question, host: done.survey.host, answer: { value: bury(yield* Schema.encodeEffect(host.survey.Schema)(done.survey)) }, asked }] : [];
    const made = [...surveyed, ...Array.getSomes(rows.map(factOf))];
    yield* Option.match(
        Option.filter(index, () => made.length > 0),
        { onNone: () => Effect.void, onSome: (held) => Facts.record(held, made) },
    );
    return { mismatches: Option.match(section, { onNone: () => 0, onSome: ({ mismatches }) => mismatches.length }), failed: done.failed } satisfies Verdict;
}, Effect.scoped);
const recompared = Effect.fn('recompared')(function* <S extends Surveyed, C extends Cased, E, P, F extends Fields>(name: string, host: Host<S, C, E, P, F>, file: string, override: Option.Option<string>, limit: Duration.Duration) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { product, Facts, indexed, batch, rows } = yield* loaded(name, host, file);
    const Results = results(host.Engine);
    const module = yield* Effect.fromOption(
        Option.orElse(override, () => Option.map(Option.fromUndefinedOr(batch.module), (held) => path.join(product, held))),
        () => HarnessError.unavailable(file)('case file names no module'),
    );
    const prefix = `${path.basename(stem(path, file))}.`;
    const found = (yield* fs.readDirectory(path.dirname(file))).filter((held) => held.startsWith(prefix) && held.endsWith(RESULTS));
    yield* found.length === 0 ? HarnessError.unavailable(file)(`no ${prefix}<version>${RESULTS} beside it`) : Effect.void;
    const judged = Effect.fnUntraced(function* (held: string) {
        const target = path.join(path.dirname(file), held);
        const recorded = yield* Effect.flatMap(fs.readFileString(target), Schema.decodeEffect(Results));
        const scratch = yield* fs.makeTempDirectoryScoped({ prefix: `${name}-compare-` });
        const engine = yield* Schema.decodeEffect(host.Engine)(recorded.engine);
        const asking: Asking<E> = { identifier: recorded.identifier, version: recorded.version, engine };
        const surveyed = Effect.fromOption(surveyOf(host, Facts, indexed, asking), () => HarnessError.unavailable(target)('facts index holds no survey of the recorded release, prove it first'));
        const installed = Effect.flatMap(host.installed, (listed) => installations(listed, [recorded.version], name));
        const { planned } = planning(host, Facts, [], asking, product, path.join(scratch, 'captures'), batch, rows);
        const hosted = Effect.gen(function* () {
            const [survey, [chosen]] = yield* Effect.all([surveyed, installed]);
            const { sendable } = yield* planned(survey);
            return { chosen, items: sendable, engine } satisfies Hosted<C>;
        });
        const evidence = (row: Row) =>
            Option.flatMap(
                Array.findFirst(recorded.cases, (outcome) => outcome.name === row.name),
                evidenceOf,
            );
        const { pairs, unproven } = pairsOf(batch, rows, evidence);
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
    return mismatches > 0 ? HarnessError.make({ reason: { _tag: 'Disagreed', mismatches }, subject }) : Array.match(failed, { onEmpty: () => Effect.void, onNonEmpty: (held) => HarnessError.unavailable(subject)(held.join('; ')) });
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const cases = Argument.Path('target', { mustExist: true }).pipe(Argument.atLeast(1), Argument.withDescription(`Product folder whose **/*${CASES} files run, or one case file, results written beside each as <name>.<version>${RESULTS}`));
const limited = Flag.String('limit').pipe(Flag.withSchema(Schema.DurationFromString), Flag.withDefault(Duration.minutes(2)), Flag.withDescription('Bound on a launch, on a case writing no record before it records hung, and on the batch-end quit before the instance is reported left running'));
const prove = <S extends Surveyed, C extends Cased, E, P, F extends Fields>(name: string, hosted: Effect.Effect<Host<S, C, E, P, F>, HarnessError>) =>
    Command.make(
        'prove',
        {
            targets: cases,
            release: Flag.String('release').pipe(Flag.atLeast(0), Flag.withDescription('Version prefix an installation reports, repeated, every installation when absent')),
            only: Flag.String('only').pipe(Flag.atLeast(0), Flag.withDescription('Case name to run, repeated')),
            limit: limited,
        },
        ({ targets: paths, release, only, limit }) =>
            Effect.gen(function* () {
                const host = yield* hosted;
                const [files, chosen] = yield* Effect.all([targets(paths), Effect.flatMap(host.installed, (rows) => installations(rows, release, name))]);
                const verdicts = yield* Effect.forEach(Array.cartesian(files, chosen), ([file, installed]) => proved(name, host, installed, file, only, limit));
                yield* judged(verdicts, name);
            }),
    );
const checked = <S extends Surveyed, C extends Cased, E, P, F extends Fields>(name: string, hosted: Effect.Effect<Host<S, C, E, P, F>, HarnessError>) =>
    Command.make('compare', { targets: cases, module: Flag.Path('module', { mustExist: true }).pipe(Flag.optional, Flag.withDescription('Module compared in place of the batch module, a distilled module among them')), limit: limited }, ({ targets: paths, module, limit }) =>
        Effect.gen(function* () {
            const [host, path, files] = yield* Effect.all([hosted, Path.Path, targets(paths)]);
            const override = Option.map(module, (held) => path.resolve(held));
            const verdicts = yield* Effect.forEach(files, (file) => recompared(name, host, file, override, limit));
            yield* judged(verdicts.flat(), name);
        }),
    );
const harness = <S extends Surveyed, C extends Cased, E, P, F extends Fields>(name: string, load: () => Promise<{ readonly Host: Host<S, C, E, P, F> }>) => {
    const hosted = Effect.map(Effect.tryPromise({ try: load, catch: HarnessError.refused(name) }), (imported) => imported.Host);
    return Command.make(name).pipe(Command.withSubcommands([prove(name, hosted), checked(name, hosted)]));
};
Command.make('harness').pipe(
    Command.withSubcommands([
        harness('acrobat', () => import('#/acrobat/host')),
        harness('blender', () => import('#/blender/host')),
        harness('illustrator', () => import('#/illustrator/host')),
        harness('indesign', () => import('#/indesign/host')),
        harness('photoshop', () => import('#/photoshop/host')),
        harness('rhino', () => import('#/rhino/host')),
    ]),
    Command.run({ version: effect.version }),
    Effect.provide(Layer.provideMerge(atelier, layer)),
    runMain,
);
