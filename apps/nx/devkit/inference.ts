import { findPackageJSON } from 'node:module';
import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateNodesResultArray, hashArray, type TargetConfiguration } from '@nx/devkit';
import { globWithWorkspaceContext, hashWithWorkspaceContext, quoteShellArg } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Option, Path, Record, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Verb = 'build' | 'check' | 'fix' | 'test' | 'install' | 'deploy' | 'upgrade' | 'clean';
type Targets = { readonly [Name in `${Verb}--${string}`]: TargetConfiguration } & { readonly [Name in Verb | 'nx-release-publish']?: TargetConfiguration };

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLATFORM = { runtime: 'node -p process.platform+process.arch' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.String, version: Schema.String, optionalDependencies: Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const line = (argv: readonly string[]) => argv.map(quoteShellArg).join(' ');

const ancestors = (path: Path.Path, folder: string): readonly string[] =>
    Array.unfold(
        Option.some(folder),
        Option.map((at: string) => [at, Option.liftPredicate(path.dirname(at), () => at !== '.')] as const),
    );

const aggregated = (targets: Targets, technologies: readonly string[]): Readonly<Record<string, TargetConfiguration>> => ({
    ...targets,
    ...Record.map(
        Array.groupBy(
            Record.toEntries<string, TargetConfiguration>(targets).filter(([name]) => name.includes('--')),
            ([name]) => name.slice(0, name.indexOf('--')),
        ),
        (members, verb): TargetConfiguration => ({
            executor: 'nx:noop',
            dependsOn: [`${verb}--*`],
            inputs: ['...', ...Array.dedupe(members.flatMap(([, member]) => member.inputs ?? []))],
            metadata: { technologies: [...technologies], description: `Runs every ${verb}--* target` },
        }),
    ),
});

const packaged = Effect.fnUntraced(function* (module: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const file = yield* Effect.fromNullishOr(findPackageJSON(module));
    return { file, folder: path.dirname(file), ...(yield* Effect.flatMap(fs.readFileString(file), Schema.decodeEffect(Manifest))) };
});

const identity = Effect.fnUntraced(function* (module: string, workspaceRoot: string) {
    const path = yield* Path.Path;
    const [plugin, shared] = yield* Effect.all([packaged(module), packaged(import.meta.url)]);
    const packages = [plugin.name, ...Record.keys(plugin.optionalDependencies), shared.name];
    const tracked = yield* Effect.promise(() => globWithWorkspaceContext(workspaceRoot, [path.relative(workspaceRoot, plugin.file)]));
    const source = yield* Effect.promise(() =>
        hashWithWorkspaceContext(
            workspaceRoot,
            [plugin, shared].map(({ folder }) => path.join(path.relative(workspaceRoot, folder), '**/*')),
        ),
    );
    const inputs: NonNullable<TargetConfiguration['inputs']> = tracked.length === 0 ? [{ externalDependencies: packages }] : [{ input: 'default', projects: packages }, { externalDependencies: [] }];
    return { name: plugin.name, folder: plugin.folder, digest: hashArray([source, plugin.version, shared.version]), inputs };
});

const reported = (nodes: CreateNodesResultArray, failures: readonly (readonly [file: string, error: Error])[]) =>
    Array.match(failures, {
        onEmpty: () => Effect.succeed(nodes),
        onNonEmpty: (errors) =>
            Effect.fail(
                new AggregateCreateNodesError(
                    errors.map(([file, error]) => [file, error]),
                    nodes,
                ),
            ),
    });

const inferred = <E extends Error>(nodes: Effect.Effect<CreateNodesResultArray, E, NodeServices>) =>
    nodes.pipe(
        Effect.mapError((error) => (error instanceof AggregateCreateNodesError ? error : new AggregateCreateNodesError([[null, error]], []))),
        Effect.provide(layer),
        Effect.runPromise,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { aggregated, ancestors, identity, inferred, line, PLATFORM, reported, type Targets, type Verb };
