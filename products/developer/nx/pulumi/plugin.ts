import { posix } from 'node:path';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesContext, type CreateNodesResult, type TargetConfiguration } from '@nx/devkit';
import { combineGlobPatterns, getFilesInDirectoryUsingContext, globWithWorkspaceContext, quoteShellArg } from '@nx/devkit/internal';
import { aggregated, anchored, EXECUTORS, manifested, namedInput, run } from '@rasm/nx/inference';
import { Array, Effect, FileSystem, Option, Predicate, pipe, Record, Schema, String } from 'effect';
import { exports as resolved } from 'resolve.exports';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REMOVED = ['pulumi', 'plugin', 'rm', '--all', '--yes'];
const CLEAN: TargetConfiguration = {
    executor: EXECUTORS.clean,
    configurations: { cache: { commands: [REMOVED] } },
    inputs: [{ externalDependencies: [] }],
    metadata: { description: `Under configuration cache, removes every plugin the Pulumi CLI downloaded into its home with ${REMOVED.join(' ')}`, help: { command: 'pulumi plugin rm --help', example: { args: [] } } },
};
const VERBS = { install: { args: ['install'], ordered: true }, deploy: { args: ['deploy'], ordered: true }, 'dry-run': { args: ['deploy', '--operation=preview'], ordered: false } } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Dependencies = Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({})));
const Manifest = Schema.fromJsonString(
    Schema.Struct({
        name: Schema.OptionFromOptionalKey(Schema.String),
        bin: Schema.optionalKey(Schema.Union([Schema.String, Schema.Record(Schema.String, Schema.String)])),
        exports: Schema.optionalKey(Schema.Unknown),
        main: Schema.optionalKey(Schema.NullOr(Schema.String)),
        dependencies: Dependencies,
        devDependencies: Dependencies,
        nx: Schema.optionalKey(Schema.Struct({ metadata: Schema.optionalKey(Schema.Struct({ pulumi: Schema.optionalKey(Schema.Struct({ hosts: Schema.optionalKey(Schema.NonEmptyString) })) })) })),
    }),
).annotate({
    description:
        'The name, bin, exports, main, dependencies, and nx.metadata.pulumi.hosts of a package.json: a package depending on @pulumi/pulumi with one bin is a Pulumi Automation API program, a workspace package holding at its root the file the program names in nx.metadata.pulumi.hosts runs it for itself, and the files exports or main name are what Node loads of a workspace package the program imports',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const nodes = Effect.fnUntraced(function* (files: readonly string[], context: CreateNodesContext) {
    const fs = yield* FileSystem.FileSystem;
    const anchor = anchored(context.workspaceRoot);
    const [rows, failures] = yield* Effect.partition(
        files.filter((file) => posix.basename(file) === 'package.json' && anchor.holds(posix.dirname(file))),
        (file) => fs.readFileString(posix.join(context.workspaceRoot, file)).pipe(Effect.flatMap(Schema.decodeEffect(Manifest)), Effect.mapBoth({ onFailure: (error): [string, Error] => [file, error], onSuccess: (manifest) => [file, manifest] as const })),
        { concurrency: 'unbounded' },
    );
    const packages = Record.fromEntries(Array.getSomes(rows.map(([file, manifest]) => Option.map(manifest.name, (name) => [name, { folder: posix.dirname(file), manifest }] as const))));
    const programs = pipe(
        packages,
        Record.filter(({ manifest: { dependencies, devDependencies } }) => '@pulumi/pulumi' in { ...dependencies, ...devDependencies }),
        Record.map(({ folder, manifest: { bin, nx } }, program) => {
            const commands = Predicate.isString(bin) ? [bin] : Object.values(bin ?? {});
            return Option.map(Option.liftPredicate(commands, Predicate.isTupleOf(1)), ([entry]) => ({ program, folder, entry: posix.join(folder, entry), hosts: Option.fromUndefinedOr(nx?.metadata?.pulumi?.hosts) }));
        }),
        Record.getSomes,
    );
    const declared = (name: string) => Record.keys({ ...packages[name]?.manifest.dependencies, ...packages[name]?.manifest.devDependencies });
    const reached = (names: readonly string[], seen: ReadonlySet<string>): readonly string[] => Array.match(Array.dedupe(names.filter((name) => !seen.has(name) && Record.has(packages, name))), { onEmpty: Array.empty, onNonEmpty: (fresh) => [...fresh, ...reached(fresh.flatMap(declared), seen.union(new Set(fresh)))] });
    const imported = Record.map(programs, (_entry, name) => reached(declared(name), new Set([name])));
    const folders = new Set([
        ...Record.values(programs).map(({ entry }) => posix.dirname(entry)),
        ...Record.values(imported)
            .flat()
            .flatMap((name) => Array.fromNullishOr(packages[name]?.folder)),
    ]);
    const tracked = new Set((yield* Effect.forEach(folders, (folder) => Effect.promise(() => getFilesInDirectoryUsingContext(context.workspaceRoot, folder)), { concurrency: 'unbounded' })).flat());
    const exported = Record.map(packages, ({ folder, manifest: { exports, main } }, name) => {
        const entry = Option.liftThrowable((subpath: string) => resolved({ name, exports }, subpath) ?? []);
        const subpaths = ['.', ...(Predicate.isObject(exports) ? Record.keys(exports).filter(String.startsWith('.')) : [])];
        const loaded = exports === undefined ? [main ?? 'index.js'] : subpaths.flatMap((subpath) => Option.getOrElse(entry(subpath), Array.empty<string>));
        return loaded.map((at) => posix.join(folder, at));
    });
    const present = (at: string) => {
        const [head = '', tail = ''] = at.split('*');
        return at.includes('*') ? tracked.values().some((file) => file.startsWith(head) && file.endsWith(tail)) : tracked.has(at);
    };
    const built = Record.map(imported, (names) => !names.flatMap((name) => exported[name] ?? []).every(present));
    const hosting = Array.getSomes(Record.values(programs).map((host) => Option.map(host.hosts, (at) => [at, host] as const)));
    const patterns = hosting.map(([at]) => `**/${at}`);
    const found = new Set(Array.isReadonlyArrayNonEmpty(patterns) ? yield* Effect.promise(() => globWithWorkspaceContext(context.workspaceRoot, patterns)) : []);
    const selected = Array.getSomes(
        rows.map(([file, { name }]) => {
            const root = posix.dirname(file);
            const runs = Option.firstSomeOf([Option.flatMapNullishOr(name, (own) => programs[own]), Array.findFirst(hosting, ([at, host]) => (found.has(posix.join(root, at)) ? Option.some(host) : Option.none()))]);
            return Option.map(runs, (program) => ({ ...program, root, file }));
        }),
    );
    const stated = yield* Effect.forEach(selected, ({ root }) => manifested(context.workspaceRoot, root), { concurrency: 'unbounded' });
    const results = Array.zipWith(selected, stated, ({ root, file, program, entry, folder }, held): readonly [string, CreateNodesResult] => {
        const [named, ran, hosted] = [namedInput(root, context), namedInput(folder, context), root !== folder];
        const verbs = Record.map(VERBS, ({ args, ordered }, verb): Readonly<Record<string, TargetConfiguration>> => {
            const command = ['node', posix.relative(root, entry), ...args].map(quoteShellArg).join(' ');
            return {
                pulumi: {
                    command,
                    options: { cwd: root },
                    dependsOn: ordered ? [...(hosted || !tracked.has(entry) ? ['build--*'] : []), ...(!hosted && (!tracked.has(entry) || built[program] === true) ? ['^build--*'] : []), `^${verb}--*`, ...(hosted ? [{ projects: [program], target: `${verb}--*` }] : [])] : [],
                    parallelism: false,
                    inputs: [
                        named,
                        `^${named}`,
                        ...(hosted
                            ? [
                                  { input: ran, projects: [program] },
                                  { input: ran, projects: [program], dependencies: true as const },
                              ]
                            : []),
                        { externalDependencies: [] },
                    ],
                    metadata: { description: `Runs the Pulumi Automation API program ${hosted ? `${program} for the project` : 'of the project'} with ${command}` },
                },
            };
        });
        return [file, { projects: { [root]: { root, metadata: { technologies: ['pulumi'] }, targets: aggregated({ ...(hosted ? {} : { clean: { pulumi: CLEAN } }), ...verbs }, held, 'pulumi', anchor) } } }];
    });
    return yield* failures.length === 0 ? Effect.succeed(results) : Effect.fail(new AggregateCreateNodesError(failures, results));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [combineGlobPatterns(['**/package.json', '**/project.json']), (files, _options, context) => run(nodes(files, context))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
