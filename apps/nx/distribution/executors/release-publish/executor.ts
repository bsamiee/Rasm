import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { Array, Config, Effect, Option, Schema } from 'effect';
import { built, DistributionError } from '#/plugins/host-packages';

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
            const { project, outputs, packages } = yield* built(context);
            const version = publish.nxReleaseVersionData?.[project]?.newVersion;
            const released = packages.flatMap((held) => (held.host.publish === undefined ? [] : [{ held, publish: held.host.publish }]));
            const refused = [
                ...(released.length === 0 ? [`build wrote no publishable host package under ${outputs.join(', ') || 'its outputs, which name no path'}`] : []),
                ...released.filter(({ held }) => version !== undefined && version !== null && held.version !== version).map(({ held }) => `build wrote ${held.file} at ${held.version}, not ${version} that nx release set; build again after versioning`),
            ];
            yield* version === null
                ? Effect.logInfo(`Skipped ${project}, nx release left it unversioned`)
                : Effect.andThen(
                      Array.match(refused, { onEmpty: () => Effect.void, onNonEmpty: (reasons) => DistributionError.make({ subject: project, detail: reasons.join('; ') }) }),
                      Effect.forEach(released, ({ held, publish: channel }) => channel.run(held, { dryRun: publish.dryRun ?? dry, registry: Option.fromUndefinedOr(publish.registry) }), { discard: true }),
                  );
            return { success: true };
        }).pipe(Effect.provide(layer)),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
