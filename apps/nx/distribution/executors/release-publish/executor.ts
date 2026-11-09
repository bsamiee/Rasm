import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { interpolate } from '@nx/devkit/internal';
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
const Captured = Schema.Struct({ token: Schema.String });
const Filled = Schema.Literals(['projectName', 'version']);
const Release = Schema.fromJsonString(Schema.Struct({ data: Schema.Struct({ repository: Schema.Struct({ release: Schema.NullOr(Schema.Struct({ immutable: Schema.Boolean, releaseAssets: Schema.Struct({ nodes: Schema.Array(Schema.Struct({ name: Schema.String })) }) })) }) }) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const attached = Effect.fnUntraced(function* (bundle: Package, tag: string, workspace: string, options: Publish) {
    const asset = (yield* Path.Path).basename(bundle.file);
    const release = yield* Effect.flatMap(spawned('gh', ['api', 'graphql', '-F', 'owner={owner}', '-F', 'name={repo}', '-f', `tag=${tag}`, '-f', `asset=${asset}`, '-f', `query=${ASSETS}`], workspace), (stdout) =>
        Schema.decodeEffect(Release)(stdout).pipe(Effect.mapBoth({ onFailure: DistributionError.of(`gh api graphql release ${tag}`, 'reads an unexpected reply'), onSuccess: ({ data }) => Option.fromNullishOr(data.repository.release) })),
    );
    return yield* Match.value({ present: Option.exists(release, ({ releaseAssets }) => releaseAssets.nodes.length > 0), immutable: Option.exists(release, ({ immutable }) => immutable), dry: options.dryRun, open: Option.isSome(release) }).pipe(
        Match.when({ present: true }, () => Effect.logInfo(`Skipped ${asset}, GitHub release ${tag} already holds it`)),
        Match.when({ immutable: true }, () => DistributionError.make({ subject: tag, detail: `is an immutable GitHub release, which takes no asset after nx release publishes it, so ${asset} stays unattached` })),
        Match.when({ dry: true }, ({ open }) => Effect.logInfo(`Would run gh release upload ${tag} ${bundle.file}; GitHub release ${tag} ${open ? 'exists' : 'is created by nx release before publishing'}`)),
        Match.when({ open: false }, () => DistributionError.make({ subject: tag, detail: `has no GitHub release to attach ${asset} to; nx release creates it when a changelog sets createRelease` })),
        Match.orElse(() => Effect.andThen(spawned('gh', ['release', 'upload', tag, bundle.file], workspace), Effect.logInfo(`Attached ${asset} to GitHub release ${tag}`))),
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
            const pattern = Option.fromNullishOr(context.nxJsonConfiguration.release?.releaseTag?.pattern);
            const rebuild = Option.match(configuration, { onNone: () => `nx build ${project}`, onSome: (name) => `nx build ${project} -c ${name}` });
            const refused = [
                ...Option.match(pattern, {
                    onNone: () => ['nx.json states no release.releaseTag.pattern, which names the GitHub release each package attaches to'],
                    onSome: (text) =>
                        [...text.matchAll(TOKEN)]
                            .flatMap(({ groups }) => Option.toArray(Schema.decodeUnknownOption(Captured)(groups)))
                            .filter(({ token }) => !Schema.is(Filled)(token))
                            .map(({ token }) => `nx.json release.releaseTag.pattern ${text} names {${token}}, which an independent project release does not fill`),
                }),
                ...(packages.length === 0 ? [`no host package sits under ${Array.match(outputs, { onEmpty: () => 'its build outputs, which name no path', onNonEmpty: (paths) => paths.join(', ') })}; run ${rebuild} first`] : []),
                ...packages.filter((bundle) => version !== undefined && version !== null && bundle.version !== version).map((bundle) => `${bundle.file} is at ${bundle.version}, not ${version} that nx release set; run ${rebuild} after versioning`),
            ];
            const settings = { dryRun: publish.dryRun ?? dry, registry: Option.fromUndefinedOr(publish.registry) };
            yield* version === null
                ? Effect.logInfo(`Skipped ${project}, nx release left it unversioned`)
                : Effect.andThen(
                      Array.match(refused, { onEmpty: () => Effect.void, onNonEmpty: (reasons) => DistributionError.make({ subject: project, detail: reasons.join('; ') }) }),
                      Effect.forEach(
                          Option.toArray(pattern).flatMap((text) => packages.map((bundle) => ({ bundle, tag: interpolate(text, { projectName: project, version: bundle.version }) }))),
                          ({ bundle, tag }) => Effect.andThen(attached(bundle, tag, context.root, settings), Option.match(Option.fromUndefinedOr(bundle.host.publish), { onNone: () => Effect.void, onSome: (channel) => channel.run(bundle, settings) })),
                          { discard: true },
                      ),
                  );
            return { success: true };
        }).pipe(Effect.provide(layer)),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
