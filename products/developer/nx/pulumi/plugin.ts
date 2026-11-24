import { posix } from 'node:path';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesContext, type CreateNodesResult, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { buildPackageJsonPatterns, buildPackageJsonWorkspacesMatcher, combineGlobPatterns, getFilesInDirectoryUsingContext, type PackageJson, quoteShellArg } from '@nx/devkit/internal';
import { aggregated, manifested, namedInput, run } from '@rasm/nx-distribution/inference';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Effect, FileSystem, Match, Option, Predicate, Record, Schema } from 'effect';

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
        exports: Schema.optionalKey(Schema.Unknown),
        main: Schema.optionalKey(Schema.NullOr(Schema.String)),
        dependencies: Dependencies,
        devDependencies: Dependencies,
        nx: Schema.optionalKey(Schema.Struct({ implicitDependencies: Schema.optionalKey(Schema.Array(Schema.String)) })),
    }),
).annotate({
    description:
        'The name, bin, exports, main, dependencies, and nx.implicitDependencies of a package.json: a package depending on @pulumi/pulumi with one bin is a Pulumi Automation API program, a package depending on that program or naming it in nx.implicitDependencies runs it for itself, and the files exports or main name are what Node loads of a workspace package the program imports',
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
    const packages = Record.fromEntries(Array.getSomes(rows.map(([file, manifest]) => Option.map(Option.fromUndefinedOr(manifest.name), (name) => [name, { folder: posix.dirname(file), manifest }] as const))));
    const declared = (name: string) => Record.keys({ ...packages[name]?.manifest.dependencies, ...packages[name]?.manifest.devDependencies });
    const reached = (names: readonly string[], seen: ReadonlySet<string>): ReadonlySet<string> => Array.match(Array.dedupe(names.filter((name) => !seen.has(name) && Record.has(packages, name))), { onEmpty: () => seen, onNonEmpty: (fresh) => reached(fresh.flatMap(declared), new Set([...seen, ...fresh])) });
    const imported = Record.map(programs, (_entry, name) => [...reached(declared(name), new Set([name])).difference(new Set([name]))]);
    const tracked = new Set(
        (yield* Effect.forEach(
            Array.dedupe([
                ...Record.values(programs).map(posix.dirname),
                ...Record.values(imported)
                    .flat()
                    .flatMap((name) => Array.fromNullishOr(packages[name]?.folder)),
            ]),
            (folder) => Effect.promise(() => getFilesInDirectoryUsingContext(context.workspaceRoot, folder)),
            { concurrency: 'unbounded' },
        )).flat(),
    );
    const entries = (value: unknown): readonly string[] =>
        Match.value(value).pipe(
            Match.when(Predicate.isString, (at) => [at]),
            Match.when(Predicate.isObject, (held) => Object.values(held).flatMap(entries)),
            Match.orElse(() => []),
        );
    const exported = Record.map(packages, ({ folder, manifest: { exports, main } }) => (exports === undefined ? [main ?? 'index.js'] : entries(exports)).map((at) => posix.join(folder, at)));
    const listed = [...tracked];
    const present = (at: string) => {
        const [head = '', tail = ''] = at.split('*');
        return at.includes('*') ? listed.some((file) => file.startsWith(head) && file.endsWith(tail)) : tracked.has(at);
    };
    const built = Record.map(imported, (names) => !names.flatMap((name) => exported[name] ?? []).every(present));
    const selected = Array.getSomes(
        rows.map(([file, { name, dependencies, nx }]) =>
            Option.map(
                Array.findFirst([...Array.fromNullishOr(name), ...[...Record.keys(dependencies), ...(nx?.implicitDependencies ?? [])].toSorted()], (held) => Option.product(Option.some(held), Record.get(programs, held))),
                ([program, entry]) => [posix.dirname(file), file, entry, name === undefined || !Record.has(programs, name), built[program] === true] as const,
            ),
        ),
    );
    const stated = yield* Effect.forEach(selected, ([root]) => manifested(context.workspaceRoot, root), { concurrency: 'unbounded' });
    const results = Array.zipWith(selected, stated, ([root, file, entry, hosted, upstream], held): readonly [string, CreateNodesResult] => {
        const named = namedInput(root, context);
        const verbs = Record.map(VERBS, ({ args, ordered }, verb): Readonly<Record<string, TargetConfiguration>> => {
            const command = ['node', posix.relative(root, entry), ...args].map(quoteShellArg).join(' ');
            return {
                pulumi: {
                    command,
                    options: { cwd: root },
                    dependsOn: ordered ? [...(tracked.has(entry) && !hosted ? [] : ['build--*']), ...(!tracked.has(entry) || (!hosted && upstream) ? ['^build--*'] : []), `^${verb}--*`] : [],
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
