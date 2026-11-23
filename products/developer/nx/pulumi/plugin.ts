import { posix } from 'node:path';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesContext, type CreateNodesResult, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { buildPackageJsonPatterns, buildPackageJsonWorkspacesMatcher, combineGlobPatterns, getFilesInDirectoryUsingContext, type PackageJson, quoteShellArg } from '@nx/devkit/internal';
import { aggregated, manifested, namedInput, run } from '@rasm/nx-distribution/inference';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Effect, FileSystem, Option, Predicate, Record, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REMOVED = ['pulumi', 'plugin', 'rm', '--all', '--yes'];
const CLEAN: TargetConfiguration = {
    executor: `${distribution.name}:clean`,
    configurations: { cache: { commands: [REMOVED] } },
    inputs: [{ externalDependencies: [] }],
    metadata: { description: `Under configuration cache, removes every plugin the Pulumi CLI downloaded into its home with ${REMOVED.join(' ')}`, help: { command: 'pulumi plugin rm --help', example: { args: [] } } },
};
const VERBS = { install: { args: ['install'], ordered: true }, deploy: { args: ['deploy'], ordered: true }, 'dry-run': { args: ['deploy', '--operation=preview'], ordered: false } } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Dependencies = Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({})));
const Manifest = Schema.fromJsonString(
    Schema.Struct({
        name: Schema.optionalKey(Schema.String),
        bin: Schema.optionalKey(Schema.Union([Schema.String, Schema.Record(Schema.String, Schema.String)])),
        dependencies: Dependencies,
        devDependencies: Dependencies,
        nx: Schema.optionalKey(Schema.Struct({ implicitDependencies: Schema.optionalKey(Schema.Array(Schema.String)) })),
    }),
).annotate({
    description: 'The name, bin, dependencies, and nx.implicitDependencies of a package.json: a package depending on @pulumi/pulumi with one bin is a Pulumi Automation API program, and a package depending on that program or naming it in nx.implicitDependencies runs it for itself',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const nodes = Effect.fnUntraced(function* (files: readonly string[], context: CreateNodesContext) {
    const fs = yield* FileSystem.FileSystem;
    const member = buildPackageJsonWorkspacesMatcher(buildPackageJsonPatterns(context.workspaceRoot, (file) => readJsonFile<PackageJson>(posix.join(context.workspaceRoot, file))));
    const roots = new Set(files.filter((file) => posix.basename(file) === 'project.json').map((file) => posix.dirname(file)));
    const [rows, failures] = yield* Effect.partition(
        files.filter((file) => posix.basename(file) === 'package.json' && (member(file) || roots.has(posix.dirname(file)))),
        (file) => fs.readFileString(posix.join(context.workspaceRoot, file)).pipe(Effect.flatMap(Schema.decodeEffect(Manifest)), Effect.mapBoth({ onFailure: (error): [string, Error] => [file, error], onSuccess: (manifest) => [file, manifest] as const })),
        { concurrency: 'unbounded' },
    );
    const programs = Record.fromEntries(
        Array.getSomes(
            rows.map(([file, { name, bin, dependencies, devDependencies }]) => {
                const entries = (Predicate.isString(bin) ? [bin] : Object.values(bin ?? {})).map((entry) => posix.join(posix.dirname(file), entry));
                return Option.all([Option.fromUndefinedOr(name), Option.filter(Array.head(entries), () => entries.length === 1 && '@pulumi/pulumi' in { ...dependencies, ...devDependencies })]);
            }),
        ),
    );
    const tracked = new Set((yield* Effect.forEach(Array.dedupe(Record.values(programs).map(posix.dirname)), (folder) => Effect.promise(() => getFilesInDirectoryUsingContext(context.workspaceRoot, folder)), { concurrency: 'unbounded' })).flat());
    const selected = Array.getSomes(
        rows.map(([file, { name, dependencies, nx }]) =>
            Option.map(
                Array.findFirst([...Array.fromNullishOr(name), ...[...Record.keys(dependencies), ...(nx?.implicitDependencies ?? [])].toSorted()], (held) => Record.get(programs, held)),
                (entry) => [posix.dirname(file), file, entry, name === undefined || !Record.has(programs, name)] as const,
            ),
        ),
    );
    const stated = yield* Effect.forEach(selected, ([root]) => manifested(context.workspaceRoot, root), { concurrency: 'unbounded' });
    const results = Array.zipWith(selected, stated, ([root, file, entry, hosted], held): readonly [string, CreateNodesResult] => {
        const named = namedInput(root, context);
        const verbs = Record.map(VERBS, ({ args, ordered }, verb): Readonly<Record<string, TargetConfiguration>> => {
            const command = ['node', posix.relative(root, entry), ...args].map(quoteShellArg).join(' ');
            return {
                pulumi: {
                    command,
                    options: { cwd: root },
                    dependsOn: ordered ? [...(tracked.has(entry) ? [hosted ? 'build--*' : '^build--*'] : ['build--*', '^build--*']), `^${verb}--*`] : [],
                    parallelism: false,
                    inputs: [named, `^${named}`, { externalDependencies: [] }],
                    metadata: { description: `Runs the Pulumi Automation API program ${hosted ? 'the project depends on' : 'of the project'} with ${command}` },
                },
            };
        });
        return [file, { projects: { [root]: { root, metadata: { technologies: ['pulumi'] }, targets: aggregated({ ...(hosted ? {} : { clean: { pulumi: CLEAN } }), ...verbs }, held, 'pulumi') } } }];
    });
    return yield* failures.length === 0 ? Effect.succeed(results) : Effect.fail(new AggregateCreateNodesError(failures, results));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [combineGlobPatterns(['**/package.json', '**/project.json']), (files, _options, context) => run(nodes(files, context))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
