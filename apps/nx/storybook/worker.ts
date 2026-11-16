import { registerHooks } from 'node:module';
import { dirname } from 'node:path';
import { parentPort, workerData } from 'node:worker_threads';
import { loadConfigFile } from '@nx/devkit/internal';
import { Cause, Effect, Schema } from 'effect';
import { normalizeStories } from 'storybook/internal/common';
import type { StorybookConfig } from 'storybook/internal/types';

// --- [MODELS] --------------------------------------------------------------------------

const Main = Schema.fromJsonString(Schema.Unknown).annotate({ description: 'The framework, addons, and stories folders of a Storybook main config as JSON, functions and other values JSON drops left out' });
const Task = Schema.Tuple([Schema.String, Schema.String]).annotate({ description: 'The absolute path of a Storybook main config and the workspace root its stories folders resolve against' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.fnUntraced(function* ([main, workingDir]: typeof Task.Type) {
    const loaded = new Set<string>();
    yield* Effect.acquireRelease(
        Effect.sync(() =>
            registerHooks({
                resolve: (specifier, context, nextResolve) => {
                    const resolved = nextResolve(specifier, context);
                    loaded.add(resolved.url);
                    return resolved;
                },
            }),
        ),
        (hooks) => Effect.sync(() => hooks.deregister()),
    );
    const { framework, addons, stories } = yield* Effect.tryPromise({ try: () => loadConfigFile<StorybookConfig>(main), catch: (cause) => new Cause.UnknownError(cause, `${main} does not load as a Storybook main config`) });
    const folders = Array.isArray(stories) && stories.length > 0 ? normalizeStories(stories, { configDir: dirname(main), workingDir }) : [];
    return { main: yield* Schema.encodeEffect(Main)({ framework, addons, stories: folders }), loaded: [...loaded] };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

parentPort?.postMessage(await Effect.runPromise(Effect.scoped(Effect.flatMap(Schema.decodeUnknownEffect(Task)(workerData), read))));
