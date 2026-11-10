import type { ExecutorContext } from '@nx/devkit';
import { interpolate } from '@nx/devkit/internal';
import { printed, runtime, ToolError } from '@rasm/nx-devkit/process';
import { Array, Config, Effect, Match, Option, Path, Schema, UndefinedOr } from 'effect';
import { built, type Package, type Publish } from '#/plugins/host-packages';

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({
    registry: Schema.optionalKey(Schema.String),
    dryRun: Schema.optionalKey(Schema.Boolean),
    nxReleaseVersionData: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ newVersion: Schema.NullOr(Schema.String) }))),
});
const Release = Schema.fromJsonString(Schema.Struct({ data: Schema.Struct({ repository: Schema.Struct({ release: Schema.NullOr(Schema.Struct({ immutable: Schema.Boolean, releaseAssets: Schema.Struct({ nodes: Schema.Array(Schema.Struct({ name: Schema.String })) }) })) }) }) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const attached = Effect.fnUntraced(function* (bundle: Package, tag: string, workspace: string, options: Publish) {
    const asset = (yield* Path.Path).basename(bundle.file);
    const query = 'query($owner: String!, $name: String!, $tag: String!, $asset: String!) { repository(owner: $owner, name: $name) { release(tagName: $tag) { immutable releaseAssets(first: 1, name: $asset) { nodes { name } } } } }';
    const release = yield* Effect.flatMap(printed('gh', ['api', 'graphql', '-F', 'owner={owner}', '-F', 'name={repo}', '-f', `tag=${tag}`, '-f', `asset=${asset}`, '-f', `query=${query}`], { cwd: workspace }), (stdout) =>
        Schema.decodeEffect(Release)(stdout).pipe(Effect.mapBoth({ onFailure: ToolError.of(`gh api graphql release ${tag}`, 'reads an unexpected reply'), onSuccess: ({ data }) => Option.fromNullishOr(data.repository.release) })),
    );
    return yield* Match.value({ present: Option.exists(release, ({ releaseAssets }) => releaseAssets.nodes.length > 0), immutable: Option.exists(release, ({ immutable }) => immutable), dry: options.dryRun, open: Option.isSome(release) }).pipe(
        Match.when({ present: true }, () => Effect.logInfo(`Skipped ${asset}, GitHub release ${tag} already holds it`)),
        Match.when({ immutable: true }, () => ToolError.make({ subject: tag, detail: `is an immutable GitHub release, which takes no asset after nx release publishes it, so ${asset} stays unattached` })),
        Match.when({ dry: true }, ({ open }) => Effect.logInfo(`Would run gh release upload ${tag} ${bundle.file}; GitHub release ${tag} ${open ? 'exists' : 'is created by nx release before publishing'}`)),
        Match.when({ open: false }, () => ToolError.make({ subject: tag, detail: `has no GitHub release to attach ${asset} to; nx release creates it when a changelog sets createRelease` })),
        Match.orElse(() => Effect.andThen(printed('gh', ['release', 'upload', tag, bundle.file], { cwd: workspace }), Effect.logInfo(`Attached ${asset} to GitHub release ${tag}`))),
    );
});

const published = Effect.fnUntraced(function* (options: unknown, context: ExecutorContext) {
    const [publish, dry] = yield* Effect.all([Schema.decodeUnknownEffect(Options)(options), Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false))]);
    const configuration = Option.fromUndefinedOr(context.configurationName);
    const { project, outputs, packages } = yield* built(context, configuration, 'build');
    const version = publish.nxReleaseVersionData?.[project]?.newVersion;
    if (version === null) {
        return yield* Effect.as(Effect.logInfo(`Skipped ${project}, nx release left it unversioned`), { success: true });
    }
    const rebuild = Option.match(configuration, { onNone: () => `nx build ${project}`, onSome: (name) => `nx build ${project} -c ${name}` });
    const refused = [
        ...(packages.length === 0 ? [`no host package sits under ${Array.match(outputs, { onEmpty: () => 'its build outputs, which name no path', onNonEmpty: (paths) => paths.join(', ') })}; run ${rebuild} first`] : []),
        ...packages.filter((bundle) => version !== undefined && bundle.version !== version).map((bundle) => `${bundle.file} is at ${bundle.version}, not ${version} that nx release set; run ${rebuild} after versioning`),
    ];
    const settings = { dryRun: publish.dryRun ?? dry, registry: Option.fromUndefinedOr(publish.registry) };
    const pattern = yield* Effect.mapError(Effect.fromNullishOr(context.nxJsonConfiguration.release?.releaseTag?.pattern), ToolError.of('nx.json release.releaseTag.pattern', 'is unset, and names the GitHub release each package attaches to'));
    yield* Array.match(refused, { onEmpty: () => Effect.void, onNonEmpty: (reasons) => ToolError.make({ subject: project, detail: reasons.join('; ') }) });
    yield* Effect.forEach(packages, (bundle) => Effect.andThen(attached(bundle, interpolate(pattern, { projectName: project, version: bundle.version }), context.root, settings), UndefinedOr.match(bundle.host.publish, { onUndefined: () => Effect.void, onDefined: (channel) => channel.run(bundle, settings) })), {
        discard: true,
    });
    return { success: true };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (options: unknown, context: ExecutorContext) => published(options, context).pipe(runtime.runPromise);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
