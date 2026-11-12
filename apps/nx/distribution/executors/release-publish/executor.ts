import { attached, executor, Released, released } from '@rasm/nx-devkit/executor';
import { ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, Match, Path, UndefinedOr } from 'effect';
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
        description:
            "Attaches each host package the target's dependency targets wrote to the GitHub release of the version its manifest states through gh release upload, then publishes it to its host's store where one takes uploads, skipping a version the store holds and a project nx release left unversioned; runs through nx release publish",
    }),
    Effect.fnUntraced(function* (stated, context, project) {
        const [path, { dry, unreleased }, target] = yield* Effect.all([Path.Path, released(stated, project), Effect.fromNullishOr(context.targetName)]);
        if (unreleased) {
            return true;
        }
        const { outputs, packages } = yield* located(context, target);
        const built = yield* Array.match(packages, {
            onEmpty: () => ToolError.make({ subject: project, detail: `no host package sits under ${outputs.join(', ')}, the outputs ${target} depends on; run its dependencies first` }),
            onNonEmpty: Effect.succeed,
        });
        yield* attached(
            context,
            project,
            Array.headNonEmpty(built).version,
            Array.map(built, ({ at }) => path.relative(context.root, at)),
            dry,
        );
        yield* Effect.forEach(built, (found) => UndefinedOr.match(found.host.publish, { onUndefined: () => Effect.void, onDefined: (taking) => published(found, taking, stated.registry ?? taking.store, dry) }), { discard: true });
        return true;
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
