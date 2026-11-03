import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { Automation } from '@rasm/automation/automation';
import { Array, ByteSize, Config, Console, DateTime, Duration, Effect, FileSystem, Formatter, Layer, Logger, Match, MutableRef, Number, Option, Order, Path, Predicate, Record, References, Result, Schema, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import { Argument, Command, Flag } from 'effect/cli';
import { SecureRandom } from 'effect/crypto';
import effect from 'effect/package.json' with { type: 'json' };
import { answers, type Batch, Call, expand, Probe, type Row, scriptOf } from '#/cases';
import { compare, compared, type Pair, type Reply } from '#/compare';
import { HarnessError } from '#/error';
import { answerOf, facts } from '#/facts';
import { starts } from '#/instance';
import { Kind, logged, type Phase, partition, phase, Run, ran, signing, tagOf } from '#/log';
import { merged } from '#/precondition';
import { failures, instances, launched, records } from '#/query';
import { located } from '#/report';
import { type Answer, type Compare, Outcome, results } from '#/results';
import { type Cased, type Decoding, execute, type Fields, type Host, type Installed, type Instance, installations, type Plan, type Prepared, type Rebuilt, rerun, type Session, type Surveyed } from '#/session';
import { prepared } from '#/source';
import { bury, JsonObject, revive } from '#/values';

// --- [TYPES] ---------------------------------------------------------------------------

type Facts = ReturnType<typeof facts>;
type Fact = Facts['Fact']['Type'];
type Asking<T> = Pick<Fact, 'identifier' | 'version'> & { readonly engine: T };
type Item<C extends Cased> = Prepared<C> & { readonly row: Row };
interface Planned<C extends Cased> extends Plan<C, unknown> {
    readonly sendable: readonly Item<C>[];
    readonly skipped: readonly { readonly row: Row; readonly index: number; readonly answer: typeof Answer.Type }[];
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
const HOSTS = {
    acrobat: () => import('#/acrobat/host'),
    blender: () => import('#/blender/host'),
    illustrator: () => import('#/illustrator/host'),
    indesign: () => import('#/indesign/host'),
    photoshop: () => import('#/photoshop/host'),
    rhino: () => import('#/rhino/host'),
} as const;

// --- [MODELS] --------------------------------------------------------------------------

const atelier = Automation.layer(Config.String('ATELIER_PATH'));
const root = Config.String('HARNESS_LOGS');
const Launched = Schema.Struct({ run: Schema.String, pid: Schema.Int, started: Schema.DateTimeUtcFromString, app: Schema.String, version: Schema.String, ended: Schema.DateTimeUtcFromString }).annotate({
    description: 'Instance a run launched with the installation and release it ran and its last record',
});

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
    const path = yield* Path.Path;
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
    return { product, Facts, index, indexed, ...(yield* expand(product, file)) };
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
        const placedIn = (file: string | null) => Option.getOrNull(Option.map(Option.fromNullOr(file), (held) => path.join(product, held)));
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
                    files: UndefinedOr.map(row.files, (files) => files.map(placedIn)),
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
            onNone: () => Effect.succeed<Item<C>>({ hosted, row, fn: Option.fromUndefinedOr(Schema.is(Probe)(row) ? undefined : row.fn), map: Option.match(scripted, { onNone: () => [], onSome: ({ map }) => map }), presses: row.presses ?? [], copies, windowed: windowedOf(host, batch)(row) }),
            onSome: HarnessError.unavailable(row.name),
        });
    });
const planning = <S extends Surveyed, C extends Cased, E, P>(host: Pick<Host<S, C, E, P>, 'precondition' | 'placed' | 'Case' | 'refusal' | 'windowed'>, Facts: Facts, indexed: readonly Fact[], asking: Asking<E>, product: string, captures: string, batch: typeof Batch.Type, rows: readonly Row[]) => {
    const failed = (row: Row) => (error: HarnessError | Schema.SchemaError) =>
        Effect.andThen(Effect.annotateLogs(Effect.logError('case refused'), { cases: [row.name], tag: tagOf(error), error: error.message }), Effect.map(Effect.orDie(Schema.decodeEffect(Outcome)({ name: row.name, completion: { _tag: 'Failed', reason: error.message } })), Result.fail));
    const keyOf = (survey: S) => (row: Row) => Option.map(expressionOf(batch, row), (expression) => ({ ...asking, question: row.name, expression, args: argsOf(row), precondition: authored(batch, row), host: survey.host }));
    const placing = (survey: S) => (numbered: readonly (readonly [Row, number])[]) => {
        const placed = prepare(host, product, captures, batch, host.precondition(survey), asking.engine);
        return Effect.forEach(numbered, ([row, index]) => placed(row, index).pipe(Effect.map(Result.succeed), Effect.catchTags({ HarnessError: failed(row), SchemaError: failed(row) })));
    };
    const planned = Effect.fnUntraced(function* (survey: S) {
        const fs = yield* FileSystem.FileSystem;
        yield* fs.remove(captures, { recursive: true, force: true });
        const answered = (row: Row) => Option.flatMap(keyOf(survey)(row), (key) => Option.orElse(Facts.known(indexed, key), () => Facts.faulted(indexed, key)));
        const numbered = rows.map((row, index) => [row, index] as const);
        const skipped = Array.getSomes(numbered.map(([row, index]) => Option.map(answered(row), ({ answer }) => ({ row, index, answer }))));
        const made = yield* placing(survey)(numbered.filter(([row]) => !skipped.some((held) => held.row === row)));
        return { engine: asking.engine, sendable: Array.getSuccesses(made), settled: Array.getFailures(made), skipped } satisfies Planned<C>;
    }, phase('plan'));
    return { planned, placing, keyOf, windowed: Option.exists(Array.head(rows), windowedOf(host, batch)) };
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
    const section =
        rebuilt === undefined
            ? compare(module, pairs, unproven)
            : Effect.gen(function* () {
                  const { chosen, items, engine } = yield* hosted;
                  const outcomes = yield* rerun(host, rebuilt, chosen, scratch, limit, engine, Array.getSomes(pairs.map(callOf(items, module))));
                  return compared(module, pairs, new Map(Array.getSomes(outcomes.map(replyOf))), unproven);
              });
    const reported = (held: typeof Compare.Type) =>
        Effect.annotateLogs(
            Array.match(held.mismatches, {
                onEmpty: () => Effect.logInfo('compared'),
                onNonEmpty: () => Effect.annotateLogs(Effect.logWarning('compared'), { tag: 'Disagreed' satisfies HarnessError['reason']['_tag'] }),
            }),
            { module: held.module, agreed: held.agreed, mismatched: held.mismatches.map(({ name }) => name), unproven: held.unproven },
        );
    return section.pipe(Effect.tap(reported), phase('compare'));
};

// --- [REPORTING]
const order = (rows: readonly Row[]) => Order.mapInput(Number.Order, ({ name }: typeof Outcome.Type) => rows.findIndex((row) => row.name === name));
const proved = Effect.fn('proved')(
    function* <S extends Surveyed, C extends Cased, E, P, F extends Fields>(name: string, host: Host<S, C, E, P, F>, chosen: Installed, file: string, only: readonly string[], limit: Duration.Duration) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const { product, Facts, index, indexed, batch, rows: every } = yield* loaded(name, host, file);
        const Results = results(host.Engine, host.fields);
        yield* Schema.decodeEffect(Schema.Array(Schema.Literals(every.map((row) => row.name))))(only);
        const rows = every.filter((row) => only.length === 0 || only.includes(row.name));
        const engine = yield* Effect.fromResult(host.engine(Array.dedupe(Array.getSomes(rows.map((row) => scriptOf(batch, row))))));
        const asking: Asking<E> = { identifier: chosen.identifier, version: chosen.version, engine };
        const known = surveyOf(host, Facts, indexed, asking);
        const target = `${stem(path, file)}.${chosen.version}`;
        const scratch = yield* fs.makeTempDirectoryScoped({ prefix: `${name}-harness-` });
        const { planned, placing, keyOf, windowed } = planning(host, Facts, indexed, asking, product, `${target}.captures`, batch, rows);
        const done = yield* execute(host, chosen, scratch, limit, windowed, known, planned);
        const { plan } = done;
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
                        Array.findFirst(plan.skipped, (held) => held.row.name === row.name),
                        ({ answer }): Evidence => ({ answer, before: {}, read: {} }),
                    ),
            );
        const { pairs, unproven } = pairsOf(batch, rows, evidence);
        const section = yield* UndefinedOr.match(batch.module, {
            onUndefined: () => Effect.succeedNone,
            onDefined: (module) =>
                Effect.asSome(
                    comparing(
                        host,
                        Effect.map(placing(done.survey)(plan.skipped.map((held) => [held.row, held.index] as const)), (made) => ({ chosen, items: [...plan.sendable, ...Array.getSuccesses(made)], engine })),
                        path.join(product, module),
                        pairs,
                        unproven,
                        scratch,
                        limit,
                    ),
                ),
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
        yield* Option.isNone(done.header) && outcomes.length === 0 ? Effect.void : Effect.andThen(fs.writeFileString(`${target}${RESULTS}`, `${yield* Schema.encodeEffect(Results)(written)}\n`), Effect.annotateLogs(Effect.logInfo('results written'), { file: `${target}${RESULTS}` }));
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
    },
    Effect.scoped,
    phase('prove'),
);
const recompared = Effect.fn('recompared')(function* <S extends Surveyed, C extends Cased, E, P, F extends Fields>(name: string, host: Host<S, C, E, P, F>, file: string, override: Option.Option<string>, limit: Duration.Duration) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { product, Facts, indexed, batch, rows } = yield* loaded(name, host, file);
    const Results = results(host.Engine, host.fields);
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
        const release = { version: recorded.version };
        const section = yield* Effect.annotateLogs(comparing(host, hosted, module, pairs, unproven, scratch, limit), release);
        yield* fs.writeFileString(target, `${yield* Schema.encodeEffect(Results)({ ...recorded, compare: section })}\n`);
        yield* Effect.annotateLogs(Effect.logInfo('results written'), { ...release, file: target });
        return { mismatches: section.mismatches.length, failed: Option.fromUndefinedOr(recorded.failed) } satisfies Verdict;
    }, Effect.scoped);
    return yield* Effect.forEach(found, judged);
});
const judged = Effect.fnUntraced(function* (verdicts: readonly Verdict[], subject: string) {
    const mismatches = verdicts.reduce((total, { mismatches: held }) => total + held, 0);
    const failed = Array.getSomes(verdicts.map(({ failed: held }) => held));
    return yield* mismatches > 0 ? HarnessError.make({ reason: { _tag: 'Disagreed', mismatches }, subject }) : Array.match(failed, { onEmpty: () => Effect.void, onNonEmpty: (held) => HarnessError.unavailable(subject)(held.join('; ')) });
}, Effect.catchCause(signing));

// --- [LOGS]
const reportsOf = Effect.fn('reportsOf')(function* (name: string, host: Pick<Host, 'installed' | 'logs'>, kind: typeof Kind.Type) {
    const [fs, path, folder, live, now] = yield* Effect.all([FileSystem.FileSystem, Path.Path, root, starts(['-A']), DateTime.now]);
    const rows = yield* Effect.flatMap(launched(root, name), Schema.decodeEffect(Schema.Array(Schema.fromJsonString(Launched))));
    const found = Effect.fnUntraced(
        function* (row: typeof Launched.Type) {
            const listed = yield* host.installed;
            const chosen = yield* Effect.fromOption(
                Array.findFirst(listed, ({ app, version }) => app === row.app && version === row.version),
                () => HarnessError.unavailable(`${row.app} ${row.version}`)('no installation holds the release the run recorded'),
            );
            const alive = Option.exists(Option.fromUndefinedOr(live.get(row.pid)), (at) => DateTime.Equivalence(at, row.started));
            const instance: Instance = { pid: row.pid, started: row.started, chosen, ended: alive ? now : row.ended };
            return (yield* located(host, instance)).filter((held) => held.kind === kind).map((held) => ({ ...held, instance }));
        },
        (self, row) => Effect.provideService(self, Run, { folder: partition(path, folder, name, row.run), phase: MutableRef.make<Phase>('run') }),
    );
    const shown = Effect.fnUntraced(function* ({ file, text, instance }: Effect.Success<ReturnType<typeof found>>[number]) {
        const { mtime, size } = yield* fs.stat(file);
        const render = Option.fromUndefinedOr(text);
        const contents = yield* Effect.transposeOption(Option.as(render, fs.readFileString(file)));
        const body = Option.zipWith(render, contents, (rendered, held) => rendered(held, instance));
        const line = Formatter.formatJson({ kind, pid: instance.pid, file, modified: Option.getOrUndefined(Option.map(mtime, (at) => at.toISOString())), bytes: ByteSize.toNumberUnsafe(size) });
        yield* Option.exists(body, (held) => held.trim() === '') ? Effect.void : Console.log([line, ...Option.toArray(body)].join('\n'));
    });
    yield* Effect.forEach((yield* Effect.forEach(rows, found)).flat(), shown, { discard: true });
});

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
        Effect.fn('prove')(function* ({ targets: paths, release, only, limit }) {
            const host = yield* hosted;
            const [files, chosen] = yield* Effect.all([targets(paths), Effect.flatMap(host.installed, (rows) => installations(rows, release, name))]);
            const verdicts = yield* Effect.forEach(Array.cartesian(files, chosen), ([file, installed]) => Effect.annotateLogs(proved(name, host, installed, file, only, limit), { version: installed.version, proof: file }));
            yield* judged(verdicts, name);
        }, ran),
    ).pipe(Command.provide(Layer.mergeAll(atelier, logged(name, root))));
const checked = <S extends Surveyed, C extends Cased, E, P, F extends Fields>(name: string, hosted: Effect.Effect<Host<S, C, E, P, F>, HarnessError>) =>
    Command.make(
        'compare',
        { targets: cases, module: Flag.Path('module', { mustExist: true }).pipe(Flag.optional, Flag.withDescription('Module compared in place of the batch module, a distilled module among them')), limit: limited },
        Effect.fn('compare')(function* ({ targets: paths, module, limit }) {
            const [host, path, files] = yield* Effect.all([hosted, Path.Path, targets(paths)]);
            const override = Option.map(module, (held) => path.resolve(held));
            const verdicts = yield* Effect.forEach(files, (file) => Effect.annotateLogs(recompared(name, host, file, override, limit), { proof: file }));
            yield* judged(verdicts.flat(), name);
        }, ran),
    ).pipe(Command.provide(Layer.mergeAll(atelier, logged(name, root))));
const logs = <S extends Surveyed, C extends Cased, E, P, F extends Fields>(name: string, hosted: Effect.Effect<Host<S, C, E, P, F>, HarnessError>) =>
    Command.make(
        'log',
        { kind: Argument.Literals('kind', Kind.literals).pipe(Argument.optional, Argument.withDescription(`Report or log kind printed for each instance of the run, one of ${Kind.literals.join(', ')}, one JSON line per file followed by its text`)) },
        Effect.fn('log')(function* ({ kind }) {
            yield* Option.match(kind, { onNone: () => records(root, name), onSome: (held) => Effect.flatMap(hosted, (host) => reportsOf(name, host, held)) });
        }),
    ).pipe(Command.withDescription(`Prints the current ${name} run's records as JSON Lines, the newest ${name} run of this session or else the newest ${name} run`), Command.provide(atelier));
const harness = <S extends Surveyed, C extends Cased, E, P, F extends Fields>(name: string, load: () => Promise<{ readonly Host: Host<S, C, E, P, F> }>) => {
    const hosted = Effect.map(Effect.tryPromise({ try: load, catch: HarnessError.refused(name) }), (imported) => imported.Host);
    return Command.make(name).pipe(Command.withSubcommands([prove(name, hosted), checked(name, hosted), logs(name, hosted)]));
};
const hosts = [harness('acrobat', HOSTS.acrobat), harness('blender', HOSTS.blender), harness('illustrator', HOSTS.illustrator), harness('indesign', HOSTS.indesign), harness('photoshop', HOSTS.photoshop), harness('rhino', HOSTS.rhino)];
const failing = Command.make('failures', { host: Flag.Literals('host', Struct.keys(HOSTS)).pipe(Flag.optional, Flag.withDescription('Host printed, every host when absent')) }, ({ host }) => failures(root, host)).pipe(
    Command.withDescription('Prints each failure signature per host, version, proof, and case with its last run and first later passing run'),
);
const running = Command.make('instances', {}, () => instances(root)).pipe(Command.withDescription('Prints latest record of each live instance a run launched'));
Command.make('harness').pipe(Command.withSubcommands([...hosts, failing, running]), Command.run({ version: effect.version }), Effect.provide(Layer.mergeAll(NodeServices.layer, SecureRandom.layerWebCrypto)), NodeRuntime.runMain);
