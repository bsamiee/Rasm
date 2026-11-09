import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { Array, Config, Effect, Option, Schema } from 'effect';
import { built, DistributionError, released } from '#/plugins/host-packages';

// --- [MODELS] --------------------------------------------------------------------------

const Publish = Schema.Struct({
    registry: Schema.optionalKey(Schema.String),
    dryRun: Schema.optionalKey(Schema.Boolean),
    nxReleaseVersionData: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ newVersion: Schema.NullOr(Schema.String) }))),
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (options: unknown, context: ExecutorContext) =>
    Effect.runPromise(
        Effect.gen(function* () {
            const [publish, dry] = yield* Effect.all([Schema.decodeUnknownEffect(Publish)(options), Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false))]);
            const configuration = Option.fromUndefinedOr(context.configurationName);
            const { project, outputs, packages } = yield* built(context, configuration);
            const version = publish.nxReleaseVersionData?.[project]?.newVersion;
            const rebuild = `nx build ${project}${Option.match(configuration, { onNone: () => '', onSome: (held) => ` -c ${held}` })}`;
            const refused = [
                ...(packages.length === 0 ? [`no host package sits under ${outputs.join(', ') || 'its build outputs, which name no path'}; run ${rebuild} first`] : []),
                ...packages.filter((held) => version !== undefined && version !== null && held.version !== version).map((held) => `${held.file} is at ${held.version}, not ${version} that nx release set; run ${rebuild} after versioning`),
            ];
            yield* version === null
                ? Effect.logInfo(`Skipped ${project}, nx release left it unversioned`)
                : Effect.andThen(
                      Array.match(refused, { onEmpty: () => Effect.void, onNonEmpty: (reasons) => DistributionError.make({ subject: project, detail: reasons.join('; ') }) }),
                      Effect.forEach(packages, (held) => released(context, held, { dryRun: publish.dryRun ?? dry, registry: Option.fromUndefinedOr(publish.registry) }), { discard: true }),
                  );
            return { success: true };
        }).pipe(Effect.provide(layer)),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
