import { layer } from '@effect/platform-node/NodeServices';
import { provided } from '@rasm/testing/vitest';
import { Effect, FileSystem, Path } from 'effect';
import { taken } from '#/workspace';

// --- [COMPOSITION] ---------------------------------------------------------------------

const setup = provided(
    'readings',
    Effect.gen(function* () {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const folder = yield* fs.makeTempDirectoryScoped({ prefix: 'nx-readings-' });
        const stored = Effect.fnUntraced(function* (name: string, data: string) {
            const { checked } = yield* taken(path.join(folder, data));
            const file = path.join(folder, `${name}.json`);
            yield* fs.writeFileString(file, yield* checked);
            return file;
        });
        const fresh = yield* stored('fresh', 'first');
        const again = yield* stored('again', 'second');
        const cached = yield* stored('cached', 'first');
        return { data: path.join(folder, 'first'), fresh, again, cached };
    }).pipe(Effect.provide(layer)),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { setup };
