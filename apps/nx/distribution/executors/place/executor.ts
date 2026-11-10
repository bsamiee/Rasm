import type { ExecutorContext } from '@nx/devkit';
import { runtime, ToolError } from '@rasm/nx-devkit/process';
import { Array, Config, Effect, Option, Schema } from 'effect';
import { sourced } from '#/plugins/host-packages';

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ dryRun: Schema.optionalKey(Schema.Boolean), removeData: Schema.optionalKey(Schema.Boolean), variables: Schema.optionalKey(Schema.Tuple([Schema.String, Schema.String])) });

// --- [COMPOSITION] ---------------------------------------------------------------------

const placed = Effect.fnUntraced(function* (options: unknown, context: ExecutorContext) {
    const [{ dryRun, removeData, variables }, dry, target] = yield* Effect.all([Schema.decodeUnknownEffect(Options)(options), Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false)), Effect.fromNullishOr(context.targetName)]);
    const { project, manifests } = yield* sourced(context, Option.fromUndefinedOr(context.configurationName), target);
    const placements = Array.getSomes(manifests.map(({ host, folder, names, text }) => Option.map(Option.fromUndefinedOr(host.place), (place) => place({ folder, names, text, dry: dryRun ?? dry, removeData: removeData === true, variables: Option.fromUndefinedOr(variables) }))));
    yield* Array.match(placements, {
        onEmpty: () => ToolError.make({ subject: project, detail: 'finds no plug-in manifest a host loads from a folder in what its builds wrote' }),
        onNonEmpty: (all) => Effect.all(all, { discard: true }),
    });
    return { success: true };
});

const execute = (options: unknown, context: ExecutorContext) => placed(options, context).pipe(runtime.runPromise);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
