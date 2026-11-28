import { Array, Cache, Combiner, Effect, Equal, Option, Record, Schema, Struct } from 'effect';
import { ChildProcessSpawner } from 'effect/process';
import { Chunk, describe, release } from '#/programs/carapace';
import { signatures } from '#/programs/signatures';
import { Usage } from '#/programs/usage';
import { chunks } from '#/usage/chunks';

// --- [CONSTANTS] -----------------------------------------------------------------------

const Carried = Chunk.mapFields(Struct.assign({ programs: Schema.Record(Schema.String, Schema.Unknown) })).pipe(Schema.NonEmptyArray);
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
                    onNone: () => Array.findFirst(carried, ({ programs }) => Record.get(programs, program)).pipe(Option.map(Schema.decodeUnknownEffect(Usage)), Effect.transposeOption, Effect.orDie),
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
