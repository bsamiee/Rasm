import { registerHooks } from 'node:module';
import { dirname } from 'node:path';
import { parentPort, workerData } from 'node:worker_threads';
import { Array, Cause, Effect, Schema } from 'effect';
import { getStorybookInfo, normalizeStories } from 'storybook/internal/common';

// --- [MODELS] --------------------------------------------------------------------------

const Task = Schema.Tuple([Schema.String, Schema.String]).annotate({ description: 'The absolute path of a Storybook main config and the workspace root its stories folders resolve against' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.gen(function* () {
    const [main, workingDir] = yield* Schema.decodeUnknownEffect(Task)(workerData);
    const loaded: (readonly [url: string, specifier: string, parent: string])[] = [];
    yield* Effect.acquireRelease(
        Effect.sync(() =>
            registerHooks({
                resolve: (specifier, context, nextResolve) => {
                    const resolved = nextResolve(specifier, context);
                    loaded.push(...(context.parentURL === undefined ? [] : [[resolved.url, specifier, context.parentURL] as const]));
                    return resolved;
                },
            }),
        ),
        (hooks) => Effect.sync(() => hooks.deregister()),
    );
    const configDir = dirname(main);
    const { frameworkPackage, addons, mainConfig } = yield* Effect.tryPromise({ try: () => getStorybookInfo(configDir, workingDir), catch: (cause) => new Cause.UnknownError(cause, `getStorybookInfo refuses ${main}`) });
    return { named: [...Array.fromNullishOr(frameworkPackage), ...addons], stories: Array.isArray(mainConfig.stories) && mainConfig.stories.length > 0 ? normalizeStories(mainConfig.stories, { configDir, workingDir }) : [], loaded };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

parentPort?.postMessage(await Effect.runPromise(Effect.scoped(read)));
