import { Config, Effect, Option, UndefinedOr } from 'effect';
import { run, StackError, StackRequest } from '#/executors/deploy/executor';

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = run(
    Effect.fnUntraced(function* ({ dryRun, nxReleaseVersionData }, context, project) {
        const dry = yield* Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false), Effect.orDie);
        const version = yield* UndefinedOr.match(nxReleaseVersionData?.[project], {
            onDefined: ({ newVersion }) => Effect.succeed(Option.fromNullOr(newVersion)),
            onUndefined: () => Effect.asSome(Effect.mapError(Effect.fromNullishOr(context.projectGraph.nodes[project]?.data.metadata?.js?.packageVersion), StackError.of(project, 'states no version in its package.json'))),
        });
        return Option.map(version, (released) => ({ request: StackRequest.Release({ context, version: released }), operation: dryRun === true || dry ? 'preview' : 'up' }) as const);
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
