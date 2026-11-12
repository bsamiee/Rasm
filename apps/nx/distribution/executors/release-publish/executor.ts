import { Dry, dried, executor } from '@rasm/nx-devkit/executor';
import { ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect } from 'effect';
import { FetchHttpClient } from 'effect/http';
import { located } from '#/host-packages';

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(
    Dry.annotate({ title: 'Release publish', description: "Publishes each host package the target's dependency targets wrote to its host's store through the store's own upload; runs through nx release publish" }),
    Effect.fnUntraced(function* (stated, context, project) {
        const [dry, target] = yield* Effect.all([dried(stated), Effect.fromNullishOr(context.targetName)]);
        const { outputs, packages } = yield* located(context, target);
        const stored = packages.flatMap((found) => Array.fromNullishOr(found.host.publish).map((store) => ({ found, ...store })));
        yield* Array.match(stored, {
            onEmpty: () => ToolError.make({ subject: project, detail: `no host package sits under ${outputs.join(', ')}, the outputs ${target} depends on; run its dependencies first` }),
            onNonEmpty: Effect.forEach(({ found, store, command, push }) => (dry ? Effect.logInfo(`Would publish ${found.at} to ${store} through ${command}`) : Effect.andThen(push(found, store), Effect.logInfo(`Published ${found.at} to ${store} through ${command}`))), { discard: true }),
        });
        return true;
    }, Effect.provide(FetchHttpClient.layer)),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
