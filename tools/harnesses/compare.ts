import { pathToFileURL } from 'node:url';
import { Array, type Duration, Effect, identity, Match, Option, Predicate, Record, Result, Schema, Struct } from 'effect';
import { type Batch, Call, type Case, Probe } from '#/cases';
import { answerOf } from '#/facts';
import { HarnessError, thrown } from '#/harness-error';
import { phase, record } from '#/log';
import { argsOf, type Item } from '#/plan';
import type { Answer, Compare, Outcome } from '#/results';
import { type Cased, type Harness, type Installed, type Rebuilt, rerun, type Session, type Surveyed } from '#/session';
import { bury, JsonObject, revive } from '#/values';

// --- [TYPES] ---------------------------------------------------------------------------

type Reply = Result.Result<unknown, string>;
type Difference = (typeof Compare.Type)['mismatches'][number]['differences'][number];
interface Pair {
    readonly name: string;
    readonly fn: string;
    readonly args: readonly unknown[];
    readonly host: Reply;
    readonly settled: (rebuilt: unknown) => unknown;
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

// --- [MODELS] --------------------------------------------------------------------------

const Module = Schema.Record(Schema.String, Schema.Unknown);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [DIFFERENCES]
const differences = (host: unknown, rebuilt: unknown, path = ''): readonly Difference[] =>
    Match.value([host, rebuilt] as const).pipe(
        Match.when(
            ([left, right]) => Object.is(left, right),
            () => [],
        ),
        Match.when([Array.isArray, Array.isArray], ([left, right]: readonly [readonly unknown[], readonly unknown[]]) => Array.match(differences(left.length, right.length, `${path}.length`), { onEmpty: () => left.flatMap((item, index) => differences(item, right[index], `${path}[${index}]`)), onNonEmpty: identity })),
        Match.when([Predicate.isObject, Predicate.isObject], ([left, right]) => Array.union(Object.keys(left), Object.keys(right)).flatMap((key) => differences(left[key], right[key], `${path}.${key}`))),
        Match.orElse(() => [{ path, host: bury(host), rebuilt: bury(rebuilt) }]),
    );
const observed = (pair: Pair, reply: Reply): readonly Difference[] =>
    Match.value([pair.host, reply] as const).pipe(
        Match.when([{ _tag: 'Success' }, { _tag: 'Success' }], ([host, rebuilt]) => differences(host.success, pair.settled(rebuilt.success))),
        Match.when([{ _tag: 'Failure' }, { _tag: 'Failure' }], ([host, rebuilt]) => differences(host.failure, rebuilt.failure, '.thrown')),
        Match.orElse(([host, rebuilt]) => [{ path: '.thrown', host: bury(Result.merge(host)), rebuilt: bury(Result.merge(rebuilt)) }]),
    );
const compared = (module: string, pairs: readonly Pair[], replies: ReadonlyMap<string, Reply>, unproven: readonly string[]): typeof Compare.Type => {
    const rows = Array.getSomes(pairs.map((pair) => Option.map(Option.fromUndefinedOr(replies.get(pair.name)), (reply) => ({ name: pair.name, fn: pair.fn, args: bury(pair.args), differences: observed(pair, reply) }))));
    const mismatches = rows.filter(({ differences: found }) => found.length > 0);
    return { module, agreed: rows.length - mismatches.length, mismatches, unproven: [...unproven, ...pairs.filter(({ name }) => !replies.has(name)).map(({ name }) => name)] };
};

// --- [PAIRS]
const revived = Record.map((held: Schema.Json) => revive(held));
const evidenceOf = (row: typeof Outcome.Type): Option.Option<Evidence> => Option.map(answerOf(row), (answer) => ({ answer, before: row.before ?? {}, read: row.read ?? {} }));
const paired = (batch: typeof Batch.Type, row: Case, fn: string, evidence: Evidence): Pair => {
    const expect = row.expect ?? batch.expect ?? (Schema.is(Call)(row) || Schema.is(Probe)(row) ? 'value' : 'read');
    const keys = row.keys ?? batch.keys ?? [];
    const picked = Record.filter((_: unknown, key: string) => keys.length === 0 || keys.includes(key));
    const before = revived(evidence.before);
    const read = revived(evidence.read);
    return {
        name: row.name,
        fn,
        args: argsOf(row).map((arg) => revive(arg, { before: (key) => before[key], read: (key) => read[key] })),
        host: 'thrown' in evidence.answer ? Result.fail(evidence.answer.thrown) : Result.succeed(expect === 'value' ? revive(evidence.answer.value) : picked(read)),
        settled: (rebuilt) => (expect === 'writes' && Schema.is(JsonObject)(rebuilt) ? picked({ ...before, ...rebuilt }) : rebuilt),
    };
};
const pairsOf = (batch: typeof Batch.Type, rows: readonly Case[], evidence: (row: Case) => Option.Option<Evidence>) => {
    const proven = Array.getSomes(rows.map((row) => Option.map(Option.fromUndefinedOr(Schema.is(Probe)(row) ? undefined : row.fn), (fn) => ({ row, fn }))));
    return {
        pairs: Array.getSomes(proven.map(({ row, fn }) => Option.map(evidence(row), (held) => paired(batch, row, fn, held)))),
        unproven: proven.filter(({ row }) => Option.isNone(evidence(row))).map(({ row }) => row.name),
    };
};

// --- [REPLIES]
const imported = Effect.fn('imported')(function* (module: string, pairs: readonly Pair[]) {
    const loaded = yield* Effect.tryPromise({ try: (): Promise<unknown> => import(pathToFileURL(module).href), catch: HarnessError.refused(module) }).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Module)));
    const reply = (pair: Pair): Effect.Effect<Reply, HarnessError> =>
        Option.match(Option.filter(Record.get(loaded, pair.fn), Predicate.isFunction), {
            onNone: () => Effect.fail(HarnessError.unavailable(module)(`exports no function ${pair.fn}`)),
            onSome: (made) => Effect.result(Effect.try({ try: () => made(...pair.args), catch: thrown })),
        });
    return new Map(Array.zip(pairs.map(Struct.get('name')), yield* Effect.forEach(pairs, reply)));
});
const callOf =
    <C extends Cased>(items: readonly Item<C>[], module: string) =>
    (pair: Pair) =>
        Option.map(
            Array.findFirst(items, ({ hosted }) => hosted.name === pair.name),
            ({ hosted, row, windowed }): Rebuilt<C> => ({ hosted, windowed, fn: pair.fn, args: argsOf(row), module }),
        );
const replyOf = (row: typeof Outcome.Type) => Option.map(answerOf(row), (answer): readonly [string, Reply] => [row.name, 'value' in answer ? Result.succeed(revive(answer.value)) : Result.fail(answer.thrown)]);
const compare = <S extends Surveyed, C extends Cased, X, R>(host: Session<S, C> & Pick<Harness<S, C>, 'rebuilt'>, hosted: Effect.Effect<Hosted<C>, X, R>, module: string, pairs: readonly Pair[], unproven: readonly string[], scratch: string, limit: Duration.Duration) => {
    const { rebuilt } = host;
    const asked = (questions: readonly Pair[]) =>
        rebuilt === undefined
            ? imported(module, questions)
            : Effect.gen(function* () {
                  const { chosen, items, engine } = yield* hosted;
                  const outcomes = yield* rerun(host, rebuilt, chosen, scratch, limit, engine, Array.getSomes(questions.map(callOf(items, module))));
                  return new Map(Array.getSomes(outcomes.map(replyOf)));
              });
    const replies = Array.match(pairs, { onEmpty: () => Effect.succeed(new Map<string, Reply>()), onNonEmpty: asked });
    const reported = (held: typeof Compare.Type) =>
        record(Array.isReadonlyArrayNonEmpty(held.mismatches) ? 'Warn' : 'Info', 'compared', {
            module: held.module,
            agreed: held.agreed,
            mismatched: held.mismatches.map(({ name }) => name),
            unproven: held.unproven,
            ...(Array.isReadonlyArrayNonEmpty(held.mismatches) ? { tag: 'Disagreed' satisfies HarnessError['reason']['_tag'] } : {}),
        });
    return Effect.map(replies, (held) => compared(module, pairs, held, unproven)).pipe(Effect.tap(reported), phase('compare'));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { compare, type Evidence, evidenceOf, type Hosted, pairsOf };
