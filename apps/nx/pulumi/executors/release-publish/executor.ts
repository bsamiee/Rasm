import { Config, Effect, Option } from 'effect';
import { run, StackError, StackRequest } from '#/executors/deploy/executor';

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = run(({ dryRun, nxReleaseVersionData }, context, project) =>
    Effect.gen(function* () {
        const dry = yield* Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false), Effect.orDie);
        const version = yield* Option.match(Option.fromUndefinedOr(nxReleaseVersionData?.[project]), {
            onSome: ({ newVersion }) => Effect.succeed(Option.fromNullOr(newVersion)),
            onNone: () => Effect.asSome(Effect.mapError(Effect.fromNullishOr(context.projectGraph.nodes[project]?.data.metadata?.js?.packageVersion), StackError.of(project, 'states no version in its package.json'))),
        });
        return Option.map(version, (released) => ({ request: StackRequest.Release({ context, version: released }), operation: dryRun === true || dry ? 'preview' : 'up' }) as const);
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
