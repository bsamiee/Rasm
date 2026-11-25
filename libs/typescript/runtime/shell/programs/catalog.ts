import { Array, Cache, Combiner, Effect, Equal, Option, Record, Schema } from 'effect';
import { ChildProcessSpawner } from 'effect/process';
import { Chunk, describe, release, Row } from '#/programs/carapace';
import { signatures } from '#/programs/signatures';
import { Usage } from '#/programs/usage';
import { chunks } from '#/usage/chunks';

// --- [CONSTANTS] -----------------------------------------------------------------------

const Carried = Schema.NonEmptyArray(Schema.toEncoded(Chunk));
const merger = Option.makeReducer(Combiner.make((base: Usage, over: Usage) => base.merge(over)));

// --- [OPERATIONS] ----------------------------------------------------------------------

const catalog = Effect.fnUntraced(function* (rows: readonly (typeof Usage.Encoded)[] = []) {
    const spawner = yield* ChildProcessSpawner.ChildProcessSpawner;
    const carried = yield* Effect.orDie(Schema.decodeEffect(Carried)(chunks));
    const signed = yield* Effect.forEach([...signatures, ...rows], (row) => Schema.decodeEffect(Usage)(row));
    const named = Record.map(
        Array.groupBy(
            signed.flatMap((row) => [row.name, ...row.aliases].map((name) => ({ name, row }))),
            ({ name }) => name,
        ),
        (held) => held.map(({ row }) => row).reduce((base, row) => base.merge(row)),
    );
    const [{ carapace, groups }] = carried;
    const live = yield* Effect.cached(Effect.map(release, (found) => Option.filter(found, (installed) => installed.carapace !== carapace || !Equal.equals(installed.groups, groups))));
    const usages = yield* Cache.make({
        capacity: Number.POSITIVE_INFINITY,
        lookup: Effect.fnUntraced(
            function* (program: string) {
                const exported = yield* Option.match(yield* live, {
                    onSome: ({ completers }) => (Record.has(completers, program) ? describe(program) : Effect.succeedNone),
                    onNone: () =>
                        Effect.orDie(
                            Effect.transposeOption(
                                Option.map(
                                    Array.findFirst(carried, ({ programs }) => Record.get(programs, program)),
                                    Schema.decodeEffect(Row),
                                ),
                            ),
                        ),
                });
                return merger.combine(exported, Record.get(named, program));
            },
            Effect.provideService(ChildProcessSpawner.ChildProcessSpawner, spawner),
        ),
    });
    return (program: string) => Cache.get(usages, program);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { catalog };
