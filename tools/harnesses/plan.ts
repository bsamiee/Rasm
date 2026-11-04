import { Array, Effect, FileSystem, Match, Option, Path, Predicate, Record, Result, Schema, Struct, UndefinedOr } from 'effect';
import { answers, type Batch, Call, type Case, Probe, scriptOf } from '#/cases';
import { type Asking, Fact } from '#/facts';
import { HarnessError } from '#/harness-error';
import { record, tagOf } from '#/log';
import { merged } from '#/precondition';
import { type Answer, Outcome } from '#/results';
import { phase } from '#/run';
import type { Cased, Decoding, Harness, Plan, Prepared, Surveyed } from '#/session';
import { prepared } from '#/source';
import { bury } from '#/values';

// --- [TYPES] ---------------------------------------------------------------------------

type Item<C extends Cased> = Prepared<C> & { readonly row: Case };
interface Planned<C extends Cased> extends Plan<C, unknown> {
    readonly sendable: readonly Item<C>[];
    readonly skipped: readonly { readonly row: Case; readonly index: number; readonly answer: typeof Answer.Type }[];
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const expressionOf = (batch: typeof Batch.Type, row: Case) =>
    Match.value(row).pipe(
        Match.when(Schema.is(Probe), ({ evaluate }) => Option.some(evaluate)),
        Match.when(Schema.is(Call), ({ call }) => Option.some(call)),
        Match.orElse(() => scriptOf(batch, row)),
    );
const argsOf = (row: Case) => (Schema.is(Probe)(row) ? [] : (row.args ?? []));
const authored = (batch: typeof Batch.Type, row: Case) => merged(batch.precondition ?? {}, row.precondition ?? {});
const windowedOf = (host: Pick<Harness, 'windowed'>, batch: typeof Batch.Type) => (row: Case) => host.windowed?.(authored(batch, row), row) ?? true;
const prepare = <C extends Cased, E, P>(host: Pick<Harness<Surveyed, C, E, P>, 'placed' | 'Case' | 'refusal' | 'windowed'>, product: string, captures: string, batch: typeof Batch.Type, Precondition: Decoding<P>, engine: E) =>
    Effect.fn('prepare')(function* (row: Case, index: number) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const folder = path.join(captures, String(index));
        yield* fs.makeDirectory(folder, { recursive: true });
        const decoded = yield* Schema.decodeEffect(Precondition)(authored(batch, row));
        const { precondition, copies } = yield* host.placed(decoded, folder, product);
        const placedIn = (file: string | null) => Option.getOrNull(Option.map(Option.fromNullOr(file), (held) => path.join(product, held)));
        const args = argsOf(row).map((arg) => bury(arg));
        const kind = Match.value(row).pipe(
            Match.when(Schema.is(Probe), ({ evaluate }) => ({ _tag: 'Probe' as const, evaluate })),
            Match.when(Schema.is(Call), ({ call, keywords }) => ({ _tag: 'Call' as const, call, args, ...Record.getSomes({ keywords: Option.fromUndefinedOr(keywords) }) })),
            Match.orElse(({ keywords }) => ({ _tag: 'Run' as const, args, ...Record.getSomes({ keywords: Option.fromUndefinedOr(keywords) }) })),
        );
        const lines = Option.flatMap(Option.liftPredicate(row, Schema.is(Call)), ({ lines: held }) => Option.fromUndefinedOr(held));
        const scripted = yield* Effect.transposeOption(Option.map(scriptOf(batch, row), (script) => prepared(path.join(product, script), lines)));
        const given = Record.filter(
            {
                ...Record.fromIterableWith(Struct.keys(answers), (key) => [key, row[key]] as const),
                commands: row.commands,
                until: row.until,
                files: UndefinedOr.map(row.files, (files) => files.map(placedIn)),
                plugin: UndefinedOr.map(batch.plugin, (plugin) => path.join(product, plugin)),
                source: Option.getOrUndefined(Option.map(scripted, ({ source }) => source)),
            },
            Predicate.isNotUndefined,
        );
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
const planning = <S extends Surveyed, C extends Cased, E, P>(host: Pick<Harness<S, C, E, P>, 'precondition' | 'placed' | 'Case' | 'refusal' | 'windowed'>, indexed: readonly Fact[], asking: Asking<E>, product: string, captures: string, batch: typeof Batch.Type, rows: readonly Case[]) => {
    const failed = (row: Case) => (error: HarnessError | Schema.SchemaError) =>
        Effect.andThen(record('Error', 'case ended', { case: row.name, completion: 'Failed', tag: tagOf(error), error: error.message }), Effect.map(Effect.orDie(Schema.decodeEffect(Outcome)({ name: row.name, completion: { _tag: 'Failed', reason: error.message } })), Result.fail));
    const keyOf = (survey: S) => (row: Case) => (row.until === undefined ? Option.map(expressionOf(batch, row), (expression) => ({ ...asking, question: row.name, expression, args: argsOf(row), precondition: authored(batch, row), host: survey.host })) : Option.none());
    const placing = (survey: S) => (numbered: readonly (readonly [Case, number])[]) => {
        const placed = prepare(host, product, captures, batch, host.precondition(survey), asking.engine);
        return Effect.forEach(numbered, ([row, index]) => placed(row, index).pipe(Effect.map(Result.succeed), Effect.catchTags({ HarnessError: failed(row), SchemaError: failed(row) })));
    };
    const planned = Effect.fnUntraced(function* (survey: S) {
        const fs = yield* FileSystem.FileSystem;
        yield* fs.remove(captures, { recursive: true, force: true });
        const answered = (row: Case, index: number) =>
            Result.fromOption(
                Option.map(
                    Option.flatMap(keyOf(survey)(row), (key) => Option.firstSomeOf([Fact.known(indexed, key), Fact.faulted(indexed, key)])),
                    ({ answer }) => ({ row, index, answer }),
                ),
                () => [row, index] as const,
            );
        const [skipped, unanswered] = Array.partition(rows, answered);
        const made = yield* placing(survey)(unanswered);
        return { engine: asking.engine, sendable: Array.getSuccesses(made), settled: Array.getFailures(made), skipped } satisfies Planned<C>;
    }, phase('plan'));
    return { planned, placing, keyOf, windowed: Option.exists(Array.head(rows), windowedOf(host, batch)) };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { argsOf, type Item, planning };
