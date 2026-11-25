import { Array, Cache, Cause, Effect, Equal, Option, Record, Schema, Stream } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { signatures } from '#/programs/signatures';
import { Carapace, Usage } from '#/programs/usage';
import { ShellError } from '#/syntax/script';
import { chunks } from '#/usage/chunks';

// --- [MODELS] --------------------------------------------------------------------------

const Row = Schema.fromJsonString(Usage);
const Chunks = Schema.NonEmptyArray(Schema.Struct({ carapace: Schema.String, groups: Schema.Array(Schema.String), programs: Schema.Record(Schema.String, Schema.String) }));
const Listing = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Array(Schema.Struct({ group: Schema.String }))));

// --- [OPERATIONS] ----------------------------------------------------------------------

const catalog = Effect.fnUntraced(function* (rows: readonly (typeof Usage.Encoded)[] = []) {
    const spawner = yield* ChildProcessSpawner.ChildProcessSpawner;
    const carried = yield* Effect.orDie(Schema.decodeEffect(Chunks)(chunks));
    const [{ carapace, groups }] = carried;
    const stored = new Map(carried.flatMap(({ programs }) => Record.toEntries(programs)));
    const signed = yield* Effect.forEach([...signatures, ...rows], (row) => Schema.decodeEffect(Usage)(row));
    const named = Record.map(
        Array.groupBy(
            signed.flatMap((row) => [row.name, ...row.aliases].map((name) => ({ name, row }))),
            ({ name }) => name,
        ),
        (held) => Array.tailNonEmpty(held).reduce((base, { row }) => base.merge(row), Array.headNonEmpty(held).row),
    );
    const ran = Effect.fnUntraced(function* (args: readonly string[]) {
        const handle = yield* spawner.spawn(ChildProcess.make('carapace', args, { stdin: 'ignore' }));
        const [printed, refused, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
        return yield* code === 0 ? Effect.succeed(printed) : Effect.fail(new Cause.UnknownError(refused, `carapace ${args.join(' ')} exited with ${code}`));
    }, Effect.scoped);
    const present = yield* Effect.cached(
        Effect.all([ran(['--version']), Effect.flatMap(ran(['--list']), Schema.decodeEffect(Listing))]).pipe(
            Effect.map(([version, listing]) => Option.some({ carapace: version.trim(), groups: Array.dedupe(Array.flatten(Record.values(listing)).map(({ group }) => group)).toSorted(), names: new Set(Record.keys(listing)) })),
            Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeedNone),
            Effect.mapError(ShellError.of({ _tag: 'Host' }, 'carapace --version --list')),
        ),
    );
    const exported = (program: string) =>
        ran([program, 'export']).pipe(
            Effect.flatMap((printed) => (printed.trim() === '' ? Effect.succeedNone : Effect.asSome(Schema.decodeEffect(Carapace)(printed)))),
            Effect.mapError(ShellError.of({ _tag: 'Host' }, `carapace ${program} export`)),
        );
    const usages = yield* Cache.make({
        capacity: Number.MAX_SAFE_INTEGER,
        lookup: Effect.fnUntraced(function* (program: string) {
            const live = Option.filter(yield* present, (found) => found.carapace !== carapace || !Equal.equals(found.groups, groups));
            const flagged = yield* Option.match(live, {
                onSome: ({ names }) => (names.has(program) ? exported(program) : Effect.succeedNone),
                onNone: () => Effect.transposeOption(Option.map(Option.fromUndefinedOr(stored.get(program)), (row) => Effect.orDie(Schema.decodeEffect(Row)(row)))),
            });
            return Array.match(Array.getSomes([flagged, Record.get(named, program)]), { onEmpty: Option.none<Usage>, onNonEmpty: (found) => Option.some(Array.tailNonEmpty(found).reduce((base, over) => base.merge(over), Array.headNonEmpty(found))) });
        }),
    });
    return (program: string) => Cache.get(usages, program);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { catalog };
