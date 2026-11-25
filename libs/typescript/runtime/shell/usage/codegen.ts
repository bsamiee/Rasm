import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Array, Effect, FileSystem, Order, Path, Record, Schema } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { Carapace, Usage } from '#/programs/usage';

// --- [CONSTANTS] -----------------------------------------------------------------------

const limit = 900_000;
const divider = (label: string) => `// --- [${label}] `.padEnd(90, '-');

// --- [MODELS] --------------------------------------------------------------------------

const Listing = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Array(Schema.Struct({ group: Schema.String }))));
const Chunk = Schema.fromJsonString(Schema.Struct({ carapace: Schema.String, groups: Schema.Array(Schema.String), programs: Schema.Record(Schema.String, Schema.fromJsonString(Usage)) }), { space: 4 });

// --- [COMPOSITION] ---------------------------------------------------------------------

runMain(
    Effect.gen(function* () {
        const [spawner, fs, path] = yield* Effect.all([ChildProcessSpawner.ChildProcessSpawner, FileSystem.FileSystem, Path.Path]);
        const printed = (...args: readonly string[]) => spawner.string(ChildProcess.make('carapace', args, { stdin: 'ignore', stderr: 'inherit' }));
        const [carapace, listing] = yield* Effect.all([Effect.map(printed('--version'), (text) => text.trim()), Effect.flatMap(printed('--list'), Schema.decodeEffect(Listing))]);
        const groups = Array.sort(Array.dedupe(Array.flatten(Record.values(listing)).map(({ group }) => group)), Order.String);
        const names = Array.sort(Record.keys(Record.filter(listing, (entries) => entries.some(({ group }) => group !== 'bridge'))), Order.String);
        const rows = yield* Effect.forEach(names, (name) => Effect.map(Effect.flatMap(printed(name, 'export'), Schema.decodeEffect(Carapace)), (usage) => [name, usage] as const), { concurrency: 8 });
        const sized = yield* Effect.forEach(rows, (row) => Effect.map(Schema.encodeEffect(Chunk)({ carapace, groups, programs: Record.fromEntries([row]) }), (text) => ({ row, size: text.length })));
        const [, placed] = Array.mapAccum(sized, { chunk: 1, size: 0 }, (held, { row, size }) =>
            held.size + size > limit
                ? [
                      { chunk: held.chunk + 1, size },
                      { chunk: held.chunk + 1, row },
                  ]
                : [
                      { ...held, size: held.size + size },
                      { chunk: held.chunk, row },
                  ],
        );
        const chunks = yield* Effect.forEach(Record.toEntries(Array.groupBy(placed, ({ chunk }) => `${chunk}`)), ([chunk, held]) => Effect.map(Schema.encodeEffect(Chunk)({ carapace, groups, programs: Record.fromEntries(held.map(({ row }) => row)) }), (text) => ({ file: `chunk${chunk}`, text: `${text}\n` })));
        const index = [...chunks.map(({ file }) => `import ${file} from '#/usage/${file}.json' with { type: 'json' };`), '', divider('CONSTANTS'), '', `const chunks = [${chunks.map(({ file }) => file).join(', ')}] as const;`, '', divider('EXPORTS'), '', 'export { chunks };', ''].join('\n');
        const stale = (yield* fs.readDirectory('usage')).filter((file) => file.endsWith('.json'));
        yield* Effect.forEach(stale, (file) => fs.remove(path.join('usage', file)), { discard: true });
        yield* Effect.forEach([...chunks.map(({ file, text }) => [`${file}.json`, text] as const), ['chunks.ts', index] as const], ([file, text]) => fs.writeFileString(path.join('usage', file), text), { discard: true });
        yield* Effect.logInfo(`${carapace} [${groups.join(' ')}]: ${rows.length} completers in ${chunks.map(({ file, text }) => `${file} ${text.length}`).join(', ')}`);
    }).pipe(Effect.provide(layer)),
);
