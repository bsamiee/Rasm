import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { Array, Config, Effect, Match, Option, Path, Schema } from 'effect';
import { built, DistributionError, type Package, type Publish, spawned } from '#/plugins/host-packages';

// --- [CONSTANTS] -----------------------------------------------------------------------

const TOKEN = /\{(?<token>[^}]+)\}/gu;
const ASSETS = 'query($owner: String!, $name: String!, $tag: String!, $asset: String!) { repository(owner: $owner, name: $name) { release(tagName: $tag) { immutable releaseAssets(first: 1, name: $asset) { nodes { name } } } } }';

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({
    registry: Schema.optionalKey(Schema.String),
    dryRun: Schema.optionalKey(Schema.Boolean),
    nxReleaseVersionData: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ newVersion: Schema.NullOr(Schema.String) }))),
});
const Release = Schema.fromJsonString(Schema.Struct({ data: Schema.Struct({ repository: Schema.Struct({ release: Schema.NullOr(Schema.Struct({ immutable: Schema.Boolean, releaseAssets: Schema.Struct({ nodes: Schema.Array(Schema.Struct({ name: Schema.String })) }) })) }) }) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const attached = Effect.fnUntraced(function* (held: Package, tag: string, workspace: string, options: Publish) {
    const asset = (yield* Path.Path).basename(held.file);
    const release = yield* Effect.flatMap(spawned('gh', ['api', 'graphql', '-F', 'owner={owner}', '-F', 'name={repo}', '-f', `tag=${tag}`, '-f', `asset=${asset}`, '-f', `query=${ASSETS}`], workspace), (stdout) =>
        Schema.decodeEffect(Release)(stdout).pipe(Effect.mapBoth({ onFailure: (error) => DistributionError.make({ subject: `gh api graphql release ${tag}`, detail: error.message }), onSuccess: ({ data }) => Option.fromNullishOr(data.repository.release) })),
    );
    return yield* Match.value({ held: Option.exists(release, ({ releaseAssets }) => releaseAssets.nodes.length > 0), immutable: Option.exists(release, ({ immutable }) => immutable), dry: options.dryRun, open: Option.isSome(release) }).pipe(
        Match.when({ held: true }, () => Effect.logInfo(`Skipped ${asset}, GitHub release ${tag} already holds it`)),
        Match.when({ immutable: true }, () => DistributionError.make({ subject: tag, detail: `is an immutable GitHub release, which takes no asset once published, and nx release publishes it before nx-release-publish attaches ${asset}` })),
        Match.when({ dry: true }, ({ open }) => Effect.logInfo(`Would run gh release upload ${tag} ${held.file}; GitHub release ${tag} ${open ? 'exists' : 'is created by nx release before publishing'}`)),
        Match.when({ open: false }, () => DistributionError.make({ subject: tag, detail: `has no GitHub release to attach ${asset} to; nx release creates it when a changelog sets createRelease` })),
        Match.orElse(() => Effect.andThen(spawned('gh', ['release', 'upload', tag, held.file], workspace), Effect.logInfo(`Attached ${asset} to GitHub release ${tag}`))),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (options: unknown, context: ExecutorContext) =>
    Effect.runPromise(
        Effect.gen(function* () {
            const [publish, dry] = yield* Effect.all([Schema.decodeUnknownEffect(Options)(options), Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false))]);
            const configuration = Option.fromUndefinedOr(context.configurationName);
            const { project, outputs, packages } = yield* built(context, configuration);
            const version = publish.nxReleaseVersionData?.[project]?.newVersion;
            const pattern = context.nxJsonConfiguration.release?.releaseTag?.pattern ?? '';
            const rebuild = `nx build ${project}${Option.match(configuration, { onNone: () => '', onSome: (held) => ` -c ${held}` })}`;
            const refused = [
                ...(pattern === '' ? ['nx.json states no release.releaseTag.pattern, which names the GitHub release each package attaches to'] : []),
                ...[...pattern.matchAll(TOKEN)]
                    .flatMap(({ groups }) => Option.toArray(Option.fromUndefinedOr(groups?.token)))
                    .filter((token) => token !== 'projectName' && token !== 'version')
                    .map((token) => `nx.json release.releaseTag.pattern ${pattern} names {${token}}, which an independent project release does not fill`),
                ...(packages.length === 0 ? [`no host package sits under ${outputs.join(', ') || 'its build outputs, which name no path'}; run ${rebuild} first`] : []),
                ...packages.filter((held) => version !== undefined && version !== null && held.version !== version).map((held) => `${held.file} is at ${held.version}, not ${version} that nx release set; run ${rebuild} after versioning`),
            ];
            const settings = { dryRun: publish.dryRun ?? dry, registry: Option.fromUndefinedOr(publish.registry) };
            yield* version === null
                ? Effect.logInfo(`Skipped ${project}, nx release left it unversioned`)
                : Effect.andThen(
                      Array.match(refused, { onEmpty: () => Effect.void, onNonEmpty: (reasons) => DistributionError.make({ subject: project, detail: reasons.join('; ') }) }),
                      Effect.forEach(
                          packages,
                          (held) =>
                              Effect.andThen(
                                  attached(
                                      held,
                                      pattern.replaceAll(TOKEN, (_, token: string) => (token === 'version' ? held.version : project)),
                                      context.root,
                                      settings,
                                  ),
                                  Option.match(Option.fromUndefinedOr(held.host.publish), { onNone: () => Effect.void, onSome: (channel) => channel.run(held, settings) }),
                              ),
                          { discard: true },
                      ),
                  );
            return { success: true };
        }).pipe(Effect.provide(layer)),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
