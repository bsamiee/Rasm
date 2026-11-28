import { Buffer } from 'node:buffer';
import { availableParallelism } from 'node:os';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Array, Effect, FileSystem, Option, Order, Path, Record, Schema } from 'effect';
import { Chunk, describe, release } from '#/programs/carapace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const limit = 524_288;
const divider = (label: string) => `// --- [${label}] `.padEnd(90, '-');

// --- [MODELS] --------------------------------------------------------------------------

const Written = Schema.fromJsonString(Chunk);
const Sized = Schema.fromJsonString(Chunk.fields.programs);

// --- [COMPOSITION] ---------------------------------------------------------------------

runMain(
    Effect.gen(function* () {
        const [fs, path, { carapace, groups, completers }] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Effect.flatMap(release, Effect.fromOption)]);
        const names = Array.sort(Record.keys(Record.filter(completers, (entries) => entries.some(({ group }) => group !== 'bridge'))), Order.String);
        const described = yield* Effect.forEach(
            names,
            (name) =>
                Effect.map(
                    describe(name),
                    Option.map((usage) => ({ name, usage })),
                ),
            { concurrency: availableParallelism() },
        );
        const rows = yield* Effect.forEach(Array.getSomes(described), ({ name, usage }) => Effect.map(Schema.encodeEffect(Sized)({ [name]: usage }), (text) => ({ name, usage, bytes: Buffer.byteLength(text) })));
        const [, placed] = Array.mapAccum(rows, { chunk: 1, bytes: 0 }, (held, { name, usage, bytes }) => {
            const next = held.bytes + bytes > limit ? { chunk: held.chunk + 1, bytes } : { chunk: held.chunk, bytes: held.bytes + bytes };
            return [next, { file: `chunk${next.chunk}`, name, usage }];
        });
        const chunks = yield* Effect.forEach(Record.toEntries(Array.groupBy(placed, ({ file }) => file)), ([file, held]) => Effect.map(Schema.encodeEffect(Written)({ carapace, groups, programs: Record.fromEntries(held.map(({ name, usage }) => [name, usage])) }), (text) => ({ file, text })));
        const index = [...chunks.map(({ file }) => `import ${file} from '#/usage/${file}.json' with { type: 'json' };`), '', divider('CONSTANTS'), '', `const chunks = [${chunks.map(({ file }) => file).join(', ')}] as const;`, '', divider('EXPORTS'), '', 'export { chunks };', ''].join('\n');
        yield* fs.remove('usage', { recursive: true, force: true });
        yield* fs.makeDirectory('usage', { recursive: true });
        yield* Effect.forEach([...chunks.map(({ file, text }) => [`${file}.json`, text] as const), ['chunks.ts', index] as const], ([file, text]) => fs.writeFileString(path.join('usage', file), text), { discard: true });
        yield* Effect.logInfo(`${carapace} [${groups.join(' ')}]: ${rows.length} completers in ${chunks.map(({ file, text }) => `${file} ${Buffer.byteLength(text)} bytes`).join(', ')}`);
    }).pipe(Effect.provide(layer)),
);
