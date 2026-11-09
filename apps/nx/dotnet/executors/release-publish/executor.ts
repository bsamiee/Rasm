import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { Config, Console, Effect, flow, Option, Path, Schema } from 'effect';
import { dotnet, spawned } from '#/plugins/project-graph';

// --- [MODELS] --------------------------------------------------------------------------

const Publish = Schema.Struct({
    project: Schema.String,
    registry: Schema.optionalKey(Schema.String),
    dryRun: Schema.optionalKey(Schema.Boolean),
    nxReleaseVersionData: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ newVersion: Schema.NullOr(Schema.String) }))),
});
const Packed = Schema.fromJsonString(Schema.Struct({ Properties: Schema.Struct({ PackageOutputPath: Schema.String, PackageId: Schema.String, PackageVersion: Schema.String }) }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = flow(
    Effect.fnUntraced(function* (options: unknown, context: ExecutorContext) {
        const [path, publish, dry, project] = yield* Effect.all([Path.Path, Schema.decodeUnknownEffect(Publish)(options), Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false)), Effect.fromNullishOr(context.projectName)]);
        const unversioned = Option.contains(Option.fromUndefinedOr(publish.nxReleaseVersionData?.[project]?.newVersion), null);
        const packed = unversioned ? Option.none() : Option.some((yield* Effect.flatMap(spawned(['pack', publish.project, '-getProperty:PackageOutputPath', '-getProperty:PackageId', '-getProperty:PackageVersion'], context.root), Schema.decodeEffect(Packed))).Properties);
        const pushed = Option.filter(packed, () => publish.dryRun !== true && !dry);
        return {
            success: yield* Option.match(pushed, {
                onNone: () => Effect.succeed(true),
                onSome: ({ PackageOutputPath, PackageId, PackageVersion }) =>
                    Effect.flatMap(dotnet(['nuget', 'push', path.join(PackageOutputPath, `${PackageId}.${PackageVersion}.nupkg`), '--skip-duplicate', ...(publish.registry === undefined ? [] : ['--source', publish.registry])], context.root), ({ code, stdout, stderr }) =>
                        Effect.as(Console.log([stdout, stderr].join('')), code === 0),
                    ),
            }),
        };
    }, Effect.provide(layer)),
    Effect.runPromise,
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
