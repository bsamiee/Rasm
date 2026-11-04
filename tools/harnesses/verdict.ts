import { Array, DateTime, Effect, FileSystem, Number, Option, Order, Path, Record, Schema, Struct, UndefinedOr } from 'effect';
import { CASES, type Case, expand, scriptOf } from '#/cases';
import { compare, type Evidence, evidenceOf, type Hosted, pairsOf } from '#/compare';
import { type Asking, answerOf, Fact } from '#/facts';
import { HarnessError } from '#/harness-error';
import { record } from '#/log';
import { planning } from '#/plan';
import { type Outcome, RESULTS, results } from '#/results';
import { phase, signing } from '#/run';
import { type Cased, execute, type Harness, type Installed, installations, type Surveyed } from '#/session';
import { bury } from '#/values';

// --- [TYPES] ---------------------------------------------------------------------------

interface Verdict {
    readonly mismatches: number;
    readonly failed: Option.Option<string>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [PATHS]
const stem = (path: Path.Path, file: string) => path.join(path.dirname(file), path.basename(file, CASES));
const ancestors = (path: Path.Path, folder: string): readonly string[] => (path.dirname(folder) === folder ? [folder] : [folder, ...ancestors(path, path.dirname(folder))]);
const loaded = Effect.fn('loaded')(function* (name: string, host: Pick<Harness, 'Engine'>, file: string) {
    const path = yield* Path.Path;
    const product = path.dirname(
        Option.getOrElse(
            Array.findFirst(ancestors(path, path.dirname(file)), (held) => path.basename(held) === 'proof'),
            () => file,
        ),
    );
    const segments = product.split(path.sep);
    const folder = Option.map(
        Array.findFirstIndex(segments, (segment, at) => segment === 'applications' && segments[at + 1] === name),
        (at) => path.join(segments.slice(0, at + 2).join(path.sep), 'facts'),
    );
    const index = Option.map(folder, (held) => path.join(held, `${name}-host.json`));
    const indexed = yield* Option.match(index, { onNone: () => Effect.succeed([]), onSome: (held) => Fact.load(host.Engine, held) });
    return { product, index, indexed, ...(yield* expand(product, file)) };
});
const surveyOf = <S extends Surveyed>(host: Pick<Harness<S>, 'survey'>, indexed: readonly Fact[], asking: Asking<unknown>) =>
    Option.flatMap(Fact.known(indexed, { ...asking, question: host.survey.question, expression: host.survey.question }), ({ answer }) => ('value' in answer ? Schema.decodeOption(host.survey.Schema)(answer.value) : Option.none()));

// --- [REPORTING]
const order = (rows: readonly Case[]) => Order.mapInput(Number.Order, ({ name }: typeof Outcome.Type) => rows.findIndex((row) => row.name === name));
const proved = Effect.fn('proved')(
    function* <S extends Surveyed, C extends Cased, E, P>(name: string, host: Harness<S, C, E, P>, chosen: Installed, file: string, only: readonly string[]) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const { product, index, indexed, batch, rows: every } = yield* loaded(name, host, file);
        const Results = results(host.Engine, host.Outcome);
        yield* Schema.decodeEffect(Schema.Array(Schema.Literals(every.map((row) => row.name))))(only);
        const rows = every.filter((row) => only.length === 0 || only.includes(row.name));
        const engine = yield* Effect.fromResult(host.engine(Array.dedupe(Array.getSomes(rows.map((row) => scriptOf(batch, row))))));
        const asking: Asking<E> = { ...Struct.pick(chosen, ['identifier', 'version', 'environment']), engine };
        const known = surveyOf(host, indexed, asking);
        const target = `${stem(path, file)}.${chosen.version}`;
        const scratch = yield* fs.makeTempDirectoryScoped({ prefix: `${name}-harness-` });
        const { planned, placing, keyOf, windowed } = planning(host, indexed, asking, product, `${target}.captures`, batch, rows);
        const done = yield* execute(host, chosen, scratch, batch.limit, windowed, known, planned);
        const { plan } = done;
        const asked = DateTime.formatIso(yield* DateTime.now);
        const outcomes = Array.sort(done.outcomes, order(rows));
        const named = Record.fromIterableBy(outcomes, (outcome) => outcome.name);
        const evidence = (row: Case) =>
            Option.firstSomeOf([
                Option.flatMap(Record.get(named, row.name), evidenceOf),
                Option.map(
                    Array.findFirst(plan.skipped, (held) => held.row.name === row.name),
                    ({ answer }): Evidence => ({ answer, before: {}, read: {} }),
                ),
            ]);
        const { pairs, unproven } = pairsOf(batch, rows, evidence);
        const section = yield* UndefinedOr.match(batch.module, {
            onUndefined: () => Effect.succeedNone,
            onDefined: (module) =>
                Effect.asSome(
                    compare(
                        host,
                        Effect.map(placing(done.survey)(plan.skipped.map((held) => [held.row, held.index] as const)), (placed) => ({ chosen, items: [...plan.sendable, ...Array.getSuccesses(placed)], engine })),
                        path.join(product, module),
                        pairs,
                        unproven,
                        scratch,
                        batch.limit,
                    ),
                ),
        });
        const header = Option.getOrUndefined(done.header);
        const written = {
            application: header?.application ?? chosen.name,
            ...asking,
            ...(header?.state === undefined ? {} : { state: header.state }),
            host: done.survey.host,
            shared: done.shared,
            ...Record.getSomes({ failed: done.failed }),
            answered: done.answered,
            cases: outcomes,
            ...Record.getSomes({ compare: section }),
        };
        yield* Option.isNone(done.header) && outcomes.length === 0 ? Effect.void : Effect.andThen(fs.writeFileString(`${target}${RESULTS}`, `${yield* Schema.encodeEffect(Results)(written)}\n`), record('Info', 'results written', { file: `${target}${RESULTS}` }));
        const factOf = (row: Case) => Option.zipWith(Option.flatMap(Record.get(named, row.name), answerOf), keyOf(done.survey)(row), (answer, key): Fact => ({ ...key, answer, asked }));
        const surveyed: readonly Fact[] = Option.isSome(done.header) && Option.isNone(known) ? [{ ...asking, question: host.survey.question, expression: host.survey.question, host: done.survey.host, answer: { value: bury(yield* Schema.encodeEffect(host.survey.Schema)(done.survey)) }, asked }] : [];
        const made = [...surveyed, ...Array.getSomes(rows.map(factOf))];
        yield* Option.match(
            Option.filter(index, () => made.length > 0),
            { onNone: () => Effect.void, onSome: (held) => Fact.record(host.Engine, held, made) },
        );
        return { mismatches: Option.match(section, { onNone: () => 0, onSome: ({ mismatches }) => mismatches.length }), failed: done.failed } satisfies Verdict;
    },
    Effect.scoped,
    phase('prove'),
);
const rejudged = Effect.fn('rejudged')(function* <S extends Surveyed, C extends Cased, E, P>(name: string, host: Harness<S, C, E, P>, { product, indexed, batch, rows }: Effect.Success<ReturnType<typeof loaded>>, module: string, target: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const Results = results(host.Engine, host.Outcome);
    const recorded = yield* Effect.flatMap(fs.readFileString(target), Schema.decodeEffect(Results));
    const scratch = yield* fs.makeTempDirectoryScoped({ prefix: `${name}-compare-` });
    const engine = yield* Schema.decodeEffect(host.Engine)(recorded.engine);
    const asking: Asking<E> = { ...Struct.pick(recorded, ['identifier', 'version', 'environment']), engine };
    const surveyed = Effect.fromOption(surveyOf(host, indexed, asking), () => HarnessError.unavailable(target)('facts index holds no survey of the recorded release, prove it first'));
    const installed = Effect.flatMap(host.installed, (listed) => installations(listed, [recorded.version], name));
    const { planned } = planning(host, [], asking, product, path.join(scratch, 'captures'), batch, rows);
    const hosted = Effect.gen(function* () {
        const [survey, [chosen]] = yield* Effect.all([surveyed, installed]);
        const { sendable } = yield* planned(survey);
        return { chosen: { ...Struct.omit(chosen, ['environment']), ...Struct.pick(recorded, ['environment']) }, items: sendable, engine } satisfies Hosted<C>;
    });
    const named = Record.fromIterableBy(recorded.cases, (outcome) => outcome.name);
    const { pairs, unproven } = pairsOf(batch, rows, (row) => Option.flatMap(Record.get(named, row.name), evidenceOf));
    const release = { version: recorded.version };
    const section = yield* Effect.annotateLogs(compare(host, hosted, module, pairs, unproven, scratch, batch.limit), release);
    yield* fs.writeFileString(target, `${yield* Schema.encodeEffect(Results)({ ...recorded, compare: section })}\n`);
    yield* record('Info', 'results written', { ...release, file: target });
    return { mismatches: section.mismatches.length, failed: Option.fromUndefinedOr(recorded.failed) } satisfies Verdict;
}, Effect.scoped);
const recompared = Effect.fn('recompared')(function* <S extends Surveyed, C extends Cased, E, P>(name: string, host: Harness<S, C, E, P>, file: string, override: Option.Option<string>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const held = yield* loaded(name, host, file);
    const module = yield* Effect.fromOption(Option.firstSomeOf([override, Option.map(Option.fromUndefinedOr(held.batch.module), (relative) => path.join(held.product, relative))]), () => HarnessError.unavailable(file)('case file names no module'));
    const prefix = `${path.basename(stem(path, file))}.`;
    const found = (yield* fs.readDirectory(path.dirname(file))).filter((entry) => entry.startsWith(prefix) && entry.endsWith(RESULTS));
    yield* found.length === 0 ? HarnessError.unavailable(file)(`no ${prefix}<version>${RESULTS} beside it`) : Effect.void;
    return yield* Effect.forEach(found, (entry) => rejudged(name, host, held, module, path.join(path.dirname(file), entry)));
});
const judged = Effect.fnUntraced(function* (verdicts: readonly Verdict[], subject: string) {
    const mismatches = verdicts.reduce((total, { mismatches: held }) => total + held, 0);
    const failed = Array.getSomes(verdicts.map(({ failed: held }) => held));
    return yield* mismatches > 0 ? HarnessError.make({ reason: { _tag: 'Disagreed', mismatches }, subject }) : Array.match(failed, { onEmpty: () => Effect.void, onNonEmpty: (held) => HarnessError.unavailable(subject)(held.join('; ')) });
}, Effect.catchCause(signing));

// --- [EXPORTS] -------------------------------------------------------------------------

export { judged, proved, recompared };
