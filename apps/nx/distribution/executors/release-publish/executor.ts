import { interpolate } from '@nx/devkit/internal';
import { executor, Released, released } from '@rasm/nx-devkit/executor';
import { printed, ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, Match, Option, Path, Schema, Struct, UndefinedOr } from 'effect';
import { built, type Package, type Publish } from '#/host-packages';

// --- [MODELS] --------------------------------------------------------------------------

const Options = Released.mapFields(
    Struct.assign({
        registry: Schema.optionalKey(Schema.String).annotate({ description: "Package server URL nx release publish passes as --registry, in place of the host's default server" }),
    }),
).annotate({ title: 'Release publish', description: "Attaches each host package the project's build wrote to its GitHub release and publishes it to its host's package server, skipping distributions the server holds and a project nx release left unversioned" });
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

const execute = executor(
    Options,
    Effect.fnUntraced(function* ({ registry, ...stated }, context, project) {
        const { dry, version, unversioned } = yield* released(stated, project);
        const configuration = Option.fromUndefinedOr(context.configurationName);
        const { outputs, packages } = yield* built(context, configuration, 'build');
        if (unversioned) {
            return yield* Effect.as(Effect.logInfo(`Skipped ${project}, nx release left it unversioned`), true);
        }
        const rebuild = Option.match(configuration, { onNone: () => `nx build ${project}`, onSome: (name) => `nx build ${project} -c ${name}` });
        const refused = [
            ...(packages.length === 0 ? [`no host package sits under ${Array.match(outputs, { onEmpty: () => 'its build outputs, which name no path', onNonEmpty: (paths) => paths.join(', ') })}; run ${rebuild} first`] : []),
            ...packages.filter((bundle) => Option.exists(version, (set) => bundle.version !== set)).map((bundle) => `${bundle.file} is at ${bundle.version}, not ${Option.getOrElse(version, () => bundle.version)} that nx release set; run ${rebuild} after versioning`),
        ];
        const settings = { dryRun: dry, registry: Option.fromUndefinedOr(registry) };
        const pattern = yield* Effect.mapError(Effect.fromNullishOr(context.nxJsonConfiguration.release?.releaseTag?.pattern), ToolError.of('nx.json release.releaseTag.pattern', 'is unset, and names the GitHub release each package attaches to'));
        yield* Array.match(refused, { onEmpty: () => Effect.void, onNonEmpty: (reasons) => ToolError.make({ subject: project, detail: reasons.join('; ') }) });
        yield* Effect.forEach(packages, (bundle) => Effect.andThen(attached(bundle, interpolate(pattern, { projectName: project, version: bundle.version }), context.root, settings), UndefinedOr.match(bundle.host.publish, { onUndefined: () => Effect.void, onDefined: (channel) => channel.run(bundle, settings) })), {
            discard: true,
        });
        return true;
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
