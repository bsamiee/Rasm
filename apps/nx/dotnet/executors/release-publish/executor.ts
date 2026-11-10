import type { ExecutorContext } from '@nx/devkit';
import { printed, ran, runtime } from '@rasm/nx-devkit/process';
import { Config, Console, Effect, flow, Option, Path, Redacted, Schema } from 'effect';

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
        const [path, publish, dry, key, project] = yield* Effect.all([Path.Path, Schema.decodeUnknownEffect(Publish)(options), Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false)), Config.option(Config.Redacted('NUGET_API_KEY')), Effect.fromNullishOr(context.projectName)]);
        const unversioned = Option.contains(Option.fromUndefinedOr(publish.nxReleaseVersionData?.[project]?.newVersion), null);
        const packed = unversioned ? Option.none() : Option.some((yield* Effect.flatMap(printed('dotnet', ['pack', publish.project, '--no-build', '-getProperty:PackageOutputPath', '-getProperty:PackageId', '-getProperty:PackageVersion'], { cwd: context.root }), Schema.decodeEffect(Packed))).Properties);
        const pushed = Option.filter(packed, () => publish.dryRun !== true && !dry);
        return {
            success: yield* Option.match(pushed, {
                onNone: () => Effect.succeed(true),
                onSome: ({ PackageOutputPath, PackageId, PackageVersion }) =>
                    Effect.flatMap(
                        ran(
                            'dotnet',
                            ['nuget', 'push', path.join(PackageOutputPath, `${PackageId}.${PackageVersion}.nupkg`), '--skip-duplicate', ...(publish.registry === undefined ? [] : ['--source', publish.registry]), ...Option.match(key, { onNone: () => [], onSome: (held) => ['--api-key', Redacted.value(held)] })],
                            { cwd: context.root },
                        ),
                        ({ code, stdout, stderr }) => Effect.as(Console.log([stdout, stderr].join('')), code === 0),
                    ),
            }),
        };
    }),
    runtime.runPromise,
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
