import { executor, Released, released } from '@rasm/nx-devkit/executor';
import { ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, Match, UndefinedOr } from 'effect';
import { FetchHttpClient } from 'effect/http';
import { type Found, type Host, located } from '#/host-packages';

// --- [OPERATIONS] ----------------------------------------------------------------------

const published = (found: Found, { command, held, push }: NonNullable<Host['publish']>, store: string, dry: boolean) =>
    Effect.flatMap(held(found, store), (present) =>
        Match.value({ present, dry }).pipe(
            Match.when({ present: true }, () => Effect.logInfo(`Skipped ${found.at}, ${store} already holds ${found.name} ${found.version}`)),
            Match.when({ dry: true }, () => Effect.logInfo(`Would publish ${found.at} to ${store} through ${command}`)),
            Match.orElse(() => Effect.andThen(push(found, store), Effect.logInfo(`Published ${found.at} to ${store} through ${command}`))),
        ),
    ).pipe(Effect.mapError(ToolError.of(found.at, `does not publish to ${store}`)), Effect.provide(FetchHttpClient.layer));

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(
    Released.annotate({
        title: 'Release publish',
        description: "Publishes each host package the project's build wrote to its host's store where one takes uploads, skipping a version the store holds and a project nx release left unversioned",
    }),
    Effect.fnUntraced(function* (stated, context, project) {
        const { dry, unversioned } = yield* released(stated, project);
        if (unversioned) {
            return yield* Effect.as(Effect.logInfo(`Skipped ${project}, nx release left it unversioned`), true);
        }
        const { outputs, packages } = yield* located(context, 'build');
        const rebuild = UndefinedOr.match(context.configurationName, { onUndefined: () => `nx build ${project}`, onDefined: (name) => `nx build ${project} -c ${name}` });
        yield* Array.match(packages, {
            onEmpty: () => ToolError.make({ subject: project, detail: `no host package sits under ${Array.match(outputs, { onEmpty: () => 'its build outputs, which name no path', onNonEmpty: (paths) => paths.join(', ') })}; run ${rebuild} first` }),
            onNonEmpty: () => Effect.void,
        });
        yield* Effect.forEach(packages, (found) => UndefinedOr.match(found.host.publish, { onUndefined: () => Effect.void, onDefined: (channel) => published(found, channel, stated.registry ?? channel.store, dry) }), { discard: true });
        return true;
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
