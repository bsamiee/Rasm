import { Config, Effect, Option } from 'effect';
import { run, StackError, StackRequest } from '#/executors/deploy/executor';

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = run(({ dryRun, nxReleaseVersionData }, context) =>
    Effect.gen(function* () {
        const project = yield* Effect.fromNullishOr(context.projectName).pipe(Effect.mapError(StackError.of('nx-release-publish', 'runs without a project')));
        const version = yield* Effect.fromNullishOr(nxReleaseVersionData?.[project]).pipe(Effect.mapError(StackError.of(project, 'has no version from nx release; run it through nx release publish')));
        const dry = yield* Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false), Effect.orDie);
        return Option.map(Option.fromNullOr(version.newVersion), (released) => ({ request: StackRequest.Release({ context, version: released }), operation: dryRun === true || dry ? 'preview' : 'up' }) as const);
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
