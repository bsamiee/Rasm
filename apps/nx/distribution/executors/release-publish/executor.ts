import { matchesGlob } from 'node:path';
import type { ExecutorContext } from '@nx/devkit';
import { executor, Released, released } from '@rasm/nx-devkit/executor';
import { ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, FileSystem, Match, Option, Path, Schema, UndefinedOr } from 'effect';
import { FetchHttpClient } from 'effect/http';
import { type Found, HOSTS, type Host, located } from '#/host-packages';

// --- [MODELS] --------------------------------------------------------------------------

const Versioned = Schema.Struct({ manifest: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const published = (found: Found, { command, held, push }: NonNullable<Host['publish']>, store: string, dry: boolean) =>
    Effect.flatMap(held(found, store), (present) =>
        Match.value({ present, dry }).pipe(
            Match.when({ present: true }, () => Effect.logInfo(`Skipped ${found.at}, ${store} already holds ${found.name} ${found.version}`)),
            Match.when({ dry: true }, () => Effect.logInfo(`Would publish ${found.at} to ${store} through ${command}`)),
            Match.orElse(() => Effect.andThen(push(found, store), Effect.logInfo(`Published ${found.at} to ${store} through ${command}`))),
        ),
    ).pipe(Effect.mapError(ToolError.of(found.at, `does not publish to ${store}`)), Effect.provide(FetchHttpClient.layer));

const declared = Effect.fnUntraced(function* (context: ExecutorContext, project: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const node = yield* Effect.fromNullishOr(context.projectsConfigurations.projects[project]);
    const source = Option.flatMap(Schema.decodeUnknownOption(Versioned)(node.release?.version?.versionActionsOptions), ({ manifest }) =>
        Option.map(
            Array.findFirst(HOSTS, (host) => matchesGlob(manifest, host.manifest)),
            (host) => ({ host, at: path.join(node.root, manifest) }),
        ),
    );
    const read = yield* Effect.transposeOption(Option.map(source, ({ host, at }) => Effect.flatMap(fs.readFileString(path.join(context.root, at)), (text) => host.read(text, at))));
    return Option.flatMap(Option.flatten(read), ({ version }) => Option.fromUndefinedOr(version));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(
    Released.annotate({
        title: 'Release publish',
        description: "Attaches each host package the project's build wrote to its GitHub release, then publishes it to its host's store where one takes uploads, skipping a version the store holds, a version no release tag holds under nx release publish alone, and a project nx release left unversioned",
    }),
    Effect.fnUntraced(function* (stated, context, project) {
        const { dry, version, unversioned, tagged, attached } = yield* released(stated, context, project);
        if (unversioned) {
            return yield* Effect.as(Effect.logInfo(`Skipped ${project}, nx release left it unversioned`), true);
        }
        const source = yield* Option.match(version, { onSome: Effect.succeedSome, onNone: () => declared(context, project) });
        const held = yield* Option.match(source, { onNone: () => Effect.succeed(true), onSome: tagged });
        if (!held) {
            return yield* Effect.as(Effect.logInfo(`Skipped ${project}, no release tag holds ${Option.getOrElse(source, () => 'its version')}`), true);
        }
        const { outputs, packages } = yield* located(context, 'build');
        const rebuild = UndefinedOr.match(context.configurationName, { onUndefined: () => `nx build ${project}`, onDefined: (name) => `nx build ${project} -c ${name}` });
        const refused = [
            ...(packages.length === 0 ? [`no host package sits under ${Array.match(outputs, { onEmpty: () => 'its build outputs, which name no path', onNonEmpty: (paths) => paths.join(', ') })}; run ${rebuild} first`] : []),
            ...packages.filter((found) => Option.exists(version, (set) => found.version !== set)).map((found) => `${found.at} is at ${found.version}, not ${Option.getOrElse(version, () => found.version)} that nx release set; run ${rebuild} after versioning`),
        ];
        yield* Array.match(refused, { onEmpty: () => Effect.void, onNonEmpty: (reasons) => ToolError.make({ subject: project, detail: reasons.join('; ') }) });
        yield* Effect.forEach(
            packages,
            Effect.fnUntraced(function* (found) {
                if (!(yield* tagged(found.version))) {
                    return yield* Effect.logInfo(`Skipped ${found.at}, no release tag holds ${project} ${found.version}`);
                }
                yield* attached(found.version, [found.at]);
                return yield* UndefinedOr.match(found.host.publish, { onUndefined: () => Effect.void, onDefined: (channel) => published(found, channel, stated.registry ?? channel.store, dry) });
            }),
            { discard: true },
        );
        return true;
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
