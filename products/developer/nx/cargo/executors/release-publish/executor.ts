import { pathToFileURL } from 'node:url';
import { credentialed, released } from '@rasm/nx/executor';
import { dried, executor, spawned, Words } from '@rasm/nx/inference';
import { Array, Effect, Option, Path, Schema, String } from 'effect';
import { Crate } from '#/workspace';

// --- [MODELS] --------------------------------------------------------------------------

const Publish = Schema.Struct({
    dryRun: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Prints each change and makes none, as NX_DRY_RUN=true does' }),
    cwd: Schema.String.annotate({ description: 'Cargo workspace root, relative to the Nx workspace root' }),
    package: Schema.String.annotate({ description: 'Crate to publish' }),
    packages: Crate.pipe(Schema.Array, Schema.optionalKey).annotate({ description: 'Workspace crates the crate reaches through path dependencies, published beside it under a dry run' }),
    registry: Schema.optionalKey(Schema.String).annotate({ description: 'Registry the crate names in its publish list, passed as --registry' }),
    __unparsed__: Words,
}).annotate({
    title: 'Release publish',
    description:
        "Publishes a crate with cargo publish at the version of the project's tag at the checked-out commit, the tag nx release made from release.releaseTag.pattern, where the registry token variable Cargo reads (CARGO_REGISTRY_TOKEN, or CARGO_REGISTRIES_<NAME>_TOKEN for a named registry) is set and cargo info finds no such version in the registry; names the variable where it is unset, publishes nothing where nx release released no version of the project at that commit or the registry holds it, and under a dry run runs cargo publish --dry-run over the crate and its workspace dependencies whatever the tags; runs through nx release publish",
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
    const [tagged, token] = yield* Effect.all([dry ? Effect.succeedNone : released(context), credentialed(credential)]);
    const published = yield* Option.match(
        Option.filter(tagged, () => Option.isSome(token)),
        { onNone: () => Effect.succeedNone, onSome: ({ extractedVersion }) => Effect.map(Effect.isSuccess(spawned(['cargo', 'info', `${named}@${extractedVersion}`, ...registries], { cwd })), (held) => Option.liftPredicate(extractedVersion, () => held)) },
    );
    const delivered = spawned(publish, { cwd, stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' });
    const skipped = Effect.annotateLogs(Effect.logWarning('skipped'), { package: named, unset: Option.isNone(token) ? [credential] : [], published: Option.getOrUndefined(published) });
    yield* dry ? delivered : Option.match(tagged, { onNone: () => Effect.void, onSome: () => (Option.isSome(token) && Option.isNone(published) ? delivered : skipped) });
    return { success: true };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Publish };
