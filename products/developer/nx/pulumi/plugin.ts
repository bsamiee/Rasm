import { posix } from 'node:path';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesContext, type CreateNodesResult, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { buildPackageJsonPatterns, buildPackageJsonWorkspacesMatcher, combineGlobPatterns, globWithWorkspaceContext, type PackageJson, quoteShellArg } from '@nx/devkit/internal';
import { aggregated, namedInput, run, Targets, type Verbs } from '@rasm/nx-distribution/inference';
import { Array, Effect, FileSystem, Option, Predicate, Record, Result, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PULUMI = '@pulumi/pulumi';
const VERBS = { install: { args: ['install'], ordered: true }, deploy: { args: ['deploy'], ordered: true }, 'dry-run': { args: ['deploy', '--operation=preview'], ordered: false } } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Dependencies = Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({})));
const Manifest = Schema.fromJsonString(
    Schema.Struct({
        name: Schema.optionalKey(Schema.String),
        bin: Schema.optionalKey(Schema.Union([Schema.String, Schema.Record(Schema.String, Schema.String)])),
        dependencies: Dependencies,
        devDependencies: Dependencies,
        nx: Schema.optionalKey(Schema.Struct({ implicitDependencies: Schema.optionalKey(Schema.Array(Schema.String)), targets: Schema.optionalKey(Targets) })),
    }),
).annotate({
    description:
        'The name, bin, dependencies, and nx rows of a package.json: a package depending on @pulumi/pulumi with one bin is a Pulumi Automation API program, a package depending on that program or naming it in nx.implicitDependencies runs it for itself, and Nx merges nx.targets inputs over the inferred targets after every plugin',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const programmed = ({ name, bin, dependencies, devDependencies }: typeof Manifest.Type, file: string) =>
    Option.flatMap(
        Option.filter(Option.all({ name: Option.fromUndefinedOr(name), bin: Option.fromUndefinedOr(bin) }), () => PULUMI in { ...dependencies, ...devDependencies }),
        ({ name: program, bin: stated }) =>
            Option.map(
                Predicate.isString(stated)
                    ? Option.some(stated)
                    : Option.flatMap(
                          Option.liftPredicate(Object.values(stated), (entries) => entries.length === 1),
                          Array.head,
                      ),
                (entry) => [program, posix.join(posix.dirname(file), entry)] as const,
            ),
    );

const targets = (root: string, entry: string, hosted: boolean, built: boolean, context: CreateNodesContext): Verbs => {
    const named = namedInput(root, context);
    return Record.map(VERBS, ({ args, ordered }, verb): Readonly<Record<string, TargetConfiguration>> => {
        const command = ['node', posix.relative(root, entry), ...args].map(quoteShellArg).join(' ');
        return {
            pulumi: {
                command,
                options: { cwd: root },
                dependsOn: ordered ? [...(hosted || built ? ['build--*'] : []), ...(hosted && !built ? [] : ['^build--*']), `^${verb}--*`] : [],
                parallelism: false,
                inputs: [named, `^${named}`, { externalDependencies: [] }],
                metadata: { description: `Runs the Pulumi Automation API program ${hosted ? 'the project depends on' : 'of the project'} with ${command}` },
            },
        };
    });
};

const inferred = (programs: ReadonlyMap<string, string>, tracked: ReadonlySet<string>, { name, dependencies, nx }: typeof Manifest.Type, file: string, context: CreateNodesContext): Option.Option<readonly [string, CreateNodesResult]> => {
    const root = posix.dirname(file);
    const own = Option.flatMap(Option.fromUndefinedOr(name), (held) => Option.fromUndefinedOr(programs.get(held)));
    const hosting = Array.getSomes([...Record.keys(dependencies), ...(nx?.implicitDependencies ?? [])].toSorted().map((dependency) => Option.fromUndefinedOr(programs.get(dependency))));
    return Option.map(
        Option.orElse(own, () => Array.head(hosting)),
        (entry) => [file, { projects: { [root]: { root, metadata: { technologies: ['pulumi'] }, targets: aggregated(targets(root, entry, Option.isNone(own), !tracked.has(entry), context), nx?.targets ?? {}, 'pulumi') } } }] as const,
    );
};

const nodes = Effect.fnUntraced(function* (files: readonly string[], context: CreateNodesContext) {
    const fs = yield* FileSystem.FileSystem;
    const member = buildPackageJsonWorkspacesMatcher(buildPackageJsonPatterns(context.workspaceRoot, (file) => readJsonFile<PackageJson>(posix.join(context.workspaceRoot, file))));
    const roots = new Set(files.filter((file) => posix.basename(file) === 'project.json').map((file) => posix.dirname(file)));
    const handed = files.filter((file) => posix.basename(file) === 'package.json' && (member(file) || roots.has(posix.dirname(file))));
    const rows = Array.zip(handed, yield* Effect.forEach(handed, (file) => Effect.result(Effect.flatMap(fs.readFileString(posix.join(context.workspaceRoot, file)), Schema.decodeEffect(Manifest))), { concurrency: 'unbounded' }));
    const programs = new Map(Array.getSomes(rows.map(([file, read]) => Option.flatMap(Option.getSuccess(read), (manifest) => programmed(manifest, file)))));
    const tracked = new Set(yield* Effect.promise(() => globWithWorkspaceContext(context.workspaceRoot, programs.values().toArray())));
    const results = Array.getSomes(rows.map(([file, read]) => Option.flatMap(Option.getSuccess(read), (manifest) => inferred(programs, tracked, manifest, file, context))));
    const failures = Array.filterMap(rows, ([file, read]) => Result.map(Result.flip(read), (error): [string, Error] => [file, error]));
    return yield* Array.match(failures, { onEmpty: () => Effect.succeed(results), onNonEmpty: (refused) => Effect.fail(new AggregateCreateNodesError(refused, results)) });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [combineGlobPatterns(['**/package.json', '**/project.json']), (files, _options, context) => run(nodes(files, context))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
