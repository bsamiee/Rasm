import { layer } from '@effect/platform-node/NodeServices';
import { provided } from '@rasm/testing/vitest';
import { Effect, FileSystem, Path } from 'effect';
import { called } from '#/workspace';

// --- [COMPOSITION] ---------------------------------------------------------------------

const setup = provided(
    'readings',
    Effect.gen(function* () {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const folder = yield* fs.makeTempDirectoryScoped({ prefix: 'nx-readings-' });
        const taken = Effect.fnUntraced(function* (name: string, data: string) {
            const { checked } = yield* called(path.join(folder, data), 'node', [path.join(import.meta.dirname, 'reading.ts')]);
            const file = path.join(folder, `${name}.json`);
            yield* fs.writeFileString(file, yield* checked);
            return file;
        });
        const fresh = yield* taken('fresh', 'first');
        const again = yield* taken('again', 'second');
        const cached = yield* taken('cached', 'first');
        return { data: path.join(folder, 'first'), fresh, again, cached };
    }).pipe(Effect.provide(layer)),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { setup };
