import { layer } from '@effect/platform-node/NodeServices';
import { type ExecutorContext, getOutputsForTargetAndConfiguration } from '@nx/devkit';
import { Config, Effect, flow, Path, Schema } from 'effect';
import { ChildProcess } from 'effect/process';
import { BUILD } from '#/plugins/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Publish = Schema.Struct({
    index: Schema.optionalKey(Schema.String),
    dryRun: Schema.optionalKey(Schema.Boolean),
    nxReleaseVersionData: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ newVersion: Schema.NullOr(Schema.String) }))),
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = flow(
    Effect.fnUntraced(function* (options: unknown, context: ExecutorContext) {
        const [path, publish, dry] = yield* Effect.all([Path.Path, Schema.decodeUnknownEffect(Publish)(options), Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false))]);
        const project = yield* Effect.fromNullishOr(context.projectName);
        const [distributions] = getOutputsForTargetAndConfiguration({ project, target: BUILD }, {}, yield* Effect.fromNullishOr(context.projectGraph.nodes[project]));
        return publish.nxReleaseVersionData?.[project]?.newVersion === null
            ? { success: true }
            : {
                  success:
                      (yield* Effect.scoped(
                          Effect.flatMap(
                              ChildProcess.make('uv', ['publish', path.join(yield* Effect.fromNullishOr(distributions), '*'), ...(publish.index === undefined ? [] : ['--index', publish.index]), ...(publish.dryRun === true || dry ? ['--dry-run'] : [])], {
                                  cwd: context.root,
                                  extendEnv: true,
                                  stdin: 'inherit',
                                  stdout: 'inherit',
                                  stderr: 'inherit',
                              }),
                              (handle) => handle.exitCode,
                          ),
                      )) === 0,
              };
    }, Effect.provide(layer)),
    Effect.runPromise,
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
