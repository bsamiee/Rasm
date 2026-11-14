import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { createTaskId } from '@nx/devkit/internal';
import { Array, Cause, Config, Effect, Option, Schema, type Scope, String } from 'effect';
import { getFilesForOutputsBatch } from 'nx/src/native/index';

// --- [MODELS] --------------------------------------------------------------------------

const Dry = Schema.Struct({ dryRun: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Prints each change and makes none, as NX_DRY_RUN=true does' }) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const refused = (subject: string, detail: string) => (cause?: unknown) => new Cause.UnknownError(cause, `${subject}: ${detail}`);

const dried = ({ dryRun }: typeof Dry.Type) => Effect.map(Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false)), (stated) => dryRun === true || stated);

const credentialed = (variable: string) => Effect.map(Config.option(Config.String(variable)), Option.filter(String.isNonEmpty));

const written = Effect.fnUntraced(function* ({ root, projectName, targetName, configurationName, taskGraph }: ExecutorContext) {
    const [{ tasks, dependencies }, project, target] = yield* Effect.all([Effect.fromNullishOr(taskGraph), Effect.fromNullishOr(projectName), Effect.fromNullishOr(targetName)]);
    const own = createTaskId(project, target, configurationName);
    const reached = (ids: ReadonlySet<string>): ReadonlySet<string> => {
        const through = [...ids].filter((id) => id === own || tasks[id]?.outputs.length === 0);
        const next = new Set([...ids, ...through.flatMap((id) => (dependencies[id] ?? []).map((edge) => edge.id)).filter((id) => tasks[id]?.target.project === project)]);
        return next.size === ids.size ? ids : reached(next);
    };
    const selected = [...reached(new Set([own]))].filter((id) => id !== own);
    const outputs = selected.map((id) => tasks[id]?.outputs ?? []);
    return { project, target, selected, outputs: outputs.flat(), files: Array.dedupe(getFilesForOutputsBatch(root, outputs).flat()).toSorted() };
});

const packaged = (context: ExecutorContext) =>
    Effect.flatMap(written(context), ({ project, target, selected, outputs, files }) =>
        Effect.filterOrFail(
            Effect.succeed(files),
            () => files.length > 0 || selected.length === 0,
            () => refused(project, `no file sits under ${outputs.join(', ')}, the outputs ${target} depends on; run its dependencies first`)(),
        ),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

const run = <A, E>(effect: Effect.Effect<A, E, NodeServices | Scope.Scope>) => effect.pipe(Effect.tapCause(Effect.logError), Effect.scoped, Effect.provide(layer), Effect.runPromise);

// --- [EXPORTS] -------------------------------------------------------------------------

export { credentialed, Dry, dried, packaged, refused, run, written };
