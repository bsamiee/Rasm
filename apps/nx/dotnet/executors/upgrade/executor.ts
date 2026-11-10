import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { Console, Effect, FileSystem, flow, Path, Schema } from 'effect';
import { traversal } from '#/executors/build/executor';
import { analyzer, dotnet, spawned } from '#/plugins/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Upgrade = Schema.Struct({ central: Schema.String, projects: Schema.NonEmptyArray(Schema.String), args: Schema.optionalKey(Schema.Array(Schema.String)) });

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = flow(
    Effect.fnUntraced(
        function* (options: unknown, context: ExecutorContext) {
            const [path, fs, upgrade, core] = yield* Effect.all([Path.Path, FileSystem.FileSystem, Schema.decodeUnknownEffect(Upgrade)(options), analyzer]);
            yield* Console.log(yield* spawned([core, 'upgrade', path.resolve(context.root, upgrade.central)], context.root));
            const file = path.join(yield* fs.makeTempDirectoryScoped(), 'restore.proj');
            yield* fs.writeFileString(file, yield* traversal(upgrade.projects.map((project) => path.resolve(context.root, project))));
            const restored = yield* dotnet(['restore', file, '--force-evaluate', ...(upgrade.args ?? [])], context.root);
            yield* Console.log([restored.stdout, restored.stderr].join(''));
            return { success: restored.code === 0 };
        },
        Effect.scoped,
        Effect.provide(layer),
    ),
    Effect.runPromise,
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
