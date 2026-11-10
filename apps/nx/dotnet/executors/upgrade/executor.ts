import type { ExecutorContext } from '@nx/devkit';
import { printed, ran, runtime } from '@rasm/nx-devkit/process';
import { Console, Effect, FileSystem, flow, Path, Schema } from 'effect';
import { traversal } from '#/executors/build/executor';
import { analyzer } from '#/plugins/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Upgrade = Schema.Struct({ central: Schema.String, projects: Schema.NonEmptyArray(Schema.String), args: Schema.optionalKey(Schema.Array(Schema.String)) });

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = flow(
    Effect.fnUntraced(function* (options: unknown, context: ExecutorContext) {
        const [path, fs, upgrade, core] = yield* Effect.all([Path.Path, FileSystem.FileSystem, Schema.decodeUnknownEffect(Upgrade)(options), analyzer]);
        const projects = upgrade.projects.map((project) => path.resolve(context.root, project));
        yield* Console.log(yield* printed('dotnet', [core, 'upgrade', path.resolve(context.root, upgrade.central)], { cwd: context.root, input: projects.join('\n') }));
        const file = path.join(yield* fs.makeTempDirectoryScoped(), 'restore.proj');
        yield* fs.writeFileString(file, yield* traversal(projects));
        const restored = yield* ran('dotnet', ['restore', file, '--force-evaluate', ...(upgrade.args ?? [])], { cwd: context.root });
        yield* Console.log([restored.stdout, restored.stderr].join(''));
        return { success: restored.code === 0 };
    }, Effect.scoped),
    runtime.runPromise,
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
