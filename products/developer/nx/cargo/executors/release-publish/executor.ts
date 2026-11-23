import { pathToFileURL } from 'node:url';
import type { ExecutorContext } from '@nx/devkit';
import { dried, executor, spawned, Words } from '@rasm/nx-distribution/inference';
import { Array, Cause, Config, Effect, Option, Path, Result, Schema, SemVer, String, Struct } from 'effect';
import { Crate } from '#/workspace';

// --- [MODELS] --------------------------------------------------------------------------

const Publish = Schema.Struct({
    dryRun: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Prints each change and makes none, as NX_DRY_RUN=true does' }),
    cwd: Schema.String.annotate({ description: 'Cargo workspace root, relative to the Nx workspace root' }),
    package: Schema.String.annotate({ description: 'Crate to publish' }),
    packages: Schema.optionalKey(Schema.Array(Crate)).annotate({ description: 'Workspace crates the crate reaches through path dependencies, published beside it under a dry run' }),
    registry: Schema.optionalKey(Schema.String).annotate({ description: 'Registry the crate names in its publish list, passed as --registry' }),
    __unparsed__: Words,
}).annotate({
    title: 'Release publish',
    description:
        "Publishes a crate with cargo publish at the version of the project's tag at the checked-out commit, the tag nx release made from release.releaseTag.pattern, where the registry token variable Cargo reads (CARGO_REGISTRY_TOKEN, or CARGO_REGISTRIES_<NAME>_TOKEN for a named registry) is set and cargo info finds no such version in the registry; names the variable where it is unset, publishes nothing where nx release released no version of the project at that commit or the registry holds it, and under a dry run runs cargo publish --dry-run over the crate and its workspace dependencies whatever the tags; runs through nx release publish",
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const released = Effect.fnUntraced(function* ({ root, projectName, nxJsonConfiguration: { release } }: ExecutorContext) {
    const project = yield* Effect.fromNullishOr(projectName);
    const pattern = release?.releaseTag?.pattern ?? (release?.projectsRelationship === 'independent' ? '{projectName}@{version}' : 'v{version}');
    const matcher = new RegExp(`^${pattern.replaceAll('{projectName}', project).split('{version}').map(RegExp.escape).join('(.+)')}$`, 'u');
    const tags = Array.filterMap((yield* spawned(['git', 'tag', '--points-at', 'HEAD'], { cwd: root })).split('\n'), (tag) =>
        Result.flatMap(
            Result.fromNullishOr(matcher.exec(tag)?.[1], () => tag),
            (version) => (Result.isSuccess(SemVer.parse(version)) ? Result.succeed({ tag, version }) : Result.fail(tag)),
        ),
    );
    return tags.length > 1 ? yield* new Cause.UnknownError(tags, `HEAD holds [${tags.map(Struct.get('tag')).join(', ')}], each matching ${pattern} for ${project}, in place of the one tag nx release makes per project`) : Array.head(tags);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(Publish, function* ({ dryRun, cwd: root, package: named, packages = [], registry, __unparsed__: words = [] }, context) {
    const [path, dry, node] = yield* Effect.all([Path.Path, dried(dryRun), Effect.flatMap(Effect.fromNullishOr(context.projectName), (project) => Effect.fromNullishOr(context.projectGraph.nodes[project]))]);
    const cwd = path.resolve(context.root, root);
    const members = [{ root: node.data.root, name: named }, ...(dry ? packages : [])];
    const selected = members.flatMap((member) => ['--package', `path+${pathToFileURL(path.resolve(context.root, member.root)).href}#${member.name}`]);
    const registries = Array.fromNullishOr(registry).flatMap((name) => ['--registry', name]);
    const publish = ['cargo', 'publish', ...selected, ...(dry ? ['--dry-run'] : []), '--locked', ...words] as const;
    const credential = registry === undefined ? 'CARGO_REGISTRY_TOKEN' : `CARGO_REGISTRIES_${String.kebabToSnake(registry).toUpperCase()}_TOKEN`;
    const [tagged, token] = yield* Effect.all([released(context), Effect.map(Config.option(Config.String(credential)), Option.filter(String.isNonEmpty))]);
    const missing = dry ? [] : [...(Option.isNone(tagged) ? [`HEAD holds no tag of ${node.name}, so nx release released no version of it at this commit`] : []), ...(Option.isNone(token) ? [`${credential} is not set`] : [])];
    const stored = yield* Option.match(
        Option.filter(tagged, () => !dry && missing.length === 0),
        { onNone: () => Effect.succeed(false), onSome: ({ version }) => Effect.isSuccess(spawned(['cargo', 'info', `${named}@${version}`, ...registries], { cwd })) },
    );
    yield* Array.match([...missing, ...(stored ? ['its registry holds the version of the tag at HEAD, as cargo info finds'] : [])], {
        onEmpty: () => spawned(publish, { cwd, stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' }),
        onNonEmpty: (reasons) => Effect.logWarning(`Skipped cargo publish of ${named}: ${reasons.join(', ')}`),
    });
    return { success: true };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Publish };
