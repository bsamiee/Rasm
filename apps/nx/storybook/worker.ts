import { registerHooks } from 'node:module';
import { parentPort, workerData } from 'node:worker_threads';
import { loadConfigFile } from '@nx/devkit/internal';
import { Cause, Effect, Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Main = Schema.fromJsonString(Schema.Unknown).annotate({ description: 'The framework and addons of a Storybook main config as JSON, functions and other values JSON drops left out' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.fnUntraced(function* (main: string) {
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
    const { framework, addons } = yield* Effect.tryPromise({ try: () => loadConfigFile<{ readonly framework?: unknown; readonly addons?: unknown }>(main), catch: (cause) => new Cause.UnknownError(cause, `${main} does not load as a Storybook main config`) });
    return { main: yield* Schema.encodeEffect(Main)({ framework, addons }), loaded: [...loaded] };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

parentPort?.postMessage(await Effect.runPromise(Effect.scoped(read(String(workerData)))));
