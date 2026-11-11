import { executor, Released, released } from '@rasm/nx-devkit/executor';
import { ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, Match, Option, UndefinedOr } from 'effect';
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
    Released.annotate({ title: 'Release publish', description: "Attaches each host package the project's build wrote to its GitHub release, then publishes it to its host's store where one takes uploads, skipping a version the store holds and a project nx release left unversioned" }),
    Effect.fnUntraced(function* (stated, context, project) {
        const { dry, version, unversioned, attached } = yield* released(stated, context, project);
        if (unversioned) {
            return yield* Effect.as(Effect.logInfo(`Skipped ${project}, nx release left it unversioned`), true);
        }
        const { outputs, packages } = yield* located(context, 'build');
        const rebuild = UndefinedOr.match(context.configurationName, { onUndefined: () => `nx build ${project}`, onDefined: (name) => `nx build ${project} -c ${name}` });
        const refused = [
            ...(packages.length === 0 ? [`no host package sits under ${Array.match(outputs, { onEmpty: () => 'its build outputs, which name no path', onNonEmpty: (paths) => paths.join(', ') })}; run ${rebuild} first`] : []),
            ...packages.filter((found) => Option.exists(version, (set) => found.version !== set)).map((found) => `${found.at} is at ${found.version}, not ${Option.getOrElse(version, () => found.version)} that nx release set; run ${rebuild} after versioning`),
        ];
        yield* Array.match(refused, { onEmpty: () => Effect.void, onNonEmpty: (reasons) => ToolError.make({ subject: project, detail: reasons.join('; ') }) });
        yield* Effect.forEach(packages, (found) => Effect.andThen(attached(found.version, [found.at]), UndefinedOr.match(found.host.publish, { onUndefined: () => Effect.void, onDefined: (channel) => published(found, channel, stated.registry ?? channel.store, dry) })), { discard: true });
        return true;
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
