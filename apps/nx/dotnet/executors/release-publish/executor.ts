import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { Config, Effect, Option, Path, Schema } from 'effect';
import { ChildProcess } from 'effect/process';
import { spawned } from '#/plugins/project-graph';

// --- [MODELS] --------------------------------------------------------------------------

const Publish = Schema.Struct({
    project: Schema.String,
    registry: Schema.optionalKey(Schema.String),
    dryRun: Schema.optionalKey(Schema.Boolean),
    nxReleaseVersionData: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ newVersion: Schema.NullOr(Schema.String) }))),
});
const Packed = Schema.fromJsonString(Schema.Struct({ Properties: Schema.Struct({ PackageOutputPath: Schema.String, PackageId: Schema.String, PackageVersion: Schema.String }) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const push = (args: readonly string[], cwd: string) => Effect.scoped(Effect.flatMap(ChildProcess.make('dotnet', ['nuget', 'push', ...args], { cwd, extendEnv: true, stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' }), (handle) => handle.exitCode));

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (options: unknown, context: ExecutorContext) =>
    Effect.runPromise(
        Effect.gen(function* () {
            const [path, publish, dry, project] = yield* Effect.all([Path.Path, Schema.decodeUnknownEffect(Publish)(options), Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false)), Effect.fromNullishOr(context.projectName)]);
            const unversioned = Option.contains(Option.fromUndefinedOr(publish.nxReleaseVersionData?.[project]?.newVersion), null);
            const packed = unversioned ? Option.none() : Option.some((yield* Effect.flatMap(spawned(['pack', publish.project, '--configuration', 'Release', '-getProperty:PackageOutputPath', '-getProperty:PackageId', '-getProperty:PackageVersion'], context.root), Schema.decodeEffect(Packed))).Properties);
            const pushed = Option.filter(packed, () => publish.dryRun !== true && !dry);
            return {
                success: yield* Option.match(pushed, {
                    onNone: () => Effect.succeed(true),
                    onSome: ({ PackageOutputPath, PackageId, PackageVersion }) => Effect.map(push([path.join(PackageOutputPath, `${PackageId}.${PackageVersion}.nupkg`), '--skip-duplicate', ...(publish.registry === undefined ? [] : ['--source', publish.registry])], context.root), (code) => code === 0),
                }),
            };
        }).pipe(Effect.provide(layer)),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
