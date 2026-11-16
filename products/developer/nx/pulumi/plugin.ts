import { posix } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesContext, type CreateNodesResult, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { buildPackageJsonPatterns, buildPackageJsonWorkspacesMatcher, combineGlobPatterns, getNamedInputs, globWithWorkspaceContext, mergeTargetConfigurations, type PackageJson, quoteShellArg } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Option, Predicate, Record, Result, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Verbs = Readonly<Record<string, Readonly<Record<string, TargetConfiguration>>>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const PULUMI = '@pulumi/pulumi';
const VERBS = ['install', 'deploy'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Configured = Schema.optionalKey(Schema.Record(Schema.String, Schema.Array(Schema.String)));
const Options = Schema.Struct({ configurations: Schema.Struct({ install: Configured, deploy: Configured }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))) }).annotate({
    description: 'configurations: per verb, each Nx configuration of its target and the words that configuration passes the program after the verb, absent gives the targets no configuration',
});
const Dependencies = Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({})));
const Input = Schema.declare((input: unknown): input is NonNullable<TargetConfiguration['inputs']>[number] => Predicate.isString(input) || Predicate.isObject(input));
const Manifest = Schema.fromJsonString(
    Schema.Struct({
        name: Schema.optionalKey(Schema.String),
        bin: Schema.optionalKey(Schema.Union([Schema.String, Schema.Record(Schema.String, Schema.String)])),
        dependencies: Dependencies,
        devDependencies: Dependencies,
        nx: Schema.optionalKey(Schema.Struct({ implicitDependencies: Schema.optionalKey(Schema.Array(Schema.String)), targets: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ inputs: Schema.optionalKey(Schema.mutable(Schema.Array(Input))) }))) })),
    }),
).annotate({
    description:
        'The name, bin, dependencies, and nx rows of a package.json: a package depending on @pulumi/pulumi with one bin is a Pulumi Automation API program, a package depending on that program or naming it in nx.implicitDependencies runs it for itself, and Nx merges nx.targets inputs over the inferred targets after every plugin',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const run = <A, E>(effect: Effect.Effect<A, E, FileSystem.FileSystem>) => Effect.runPromise(Effect.provide(effect, layer));

const aggregated = (verbs: Verbs, stated: Readonly<Record<string, Pick<TargetConfiguration, 'inputs'>>>, technology: string): Record<string, TargetConfiguration> =>
    Record.fromEntries(
        Record.collect(Record.filter(verbs, Predicate.not(Record.isEmptyRecord)), (verb, tools): readonly (readonly [string, TargetConfiguration])[] => [
            [
                verb,
                {
                    dependsOn: [`${verb}--*`],
                    inputs: [
                        '...',
                        ...Array.dedupe(
                            Record.collect(tools, (tool, target) => mergeTargetConfigurations(stated[`${verb}--${tool}`] ?? {}, { ...target, inputs: target.inputs ?? ['default', '^default'] }).inputs ?? [])
                                .flat()
                                .map((input) => (Predicate.isObject(input) && 'dependentTasksOutputFiles' in input ? { ...input, transitive: true } : input)),
                        ),
                    ],
                },
            ],
            ...Record.collect(tools, (tool, target): readonly [string, TargetConfiguration] => [`${verb}--${tool}`, { ...target, metadata: { technologies: [technology], ...target.metadata } }]),
        ]).flat(),
    );

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

const targets = (root: string, entry: string, hosted: boolean, built: boolean, context: CreateNodesContext, configurations: (typeof Options.Type)['configurations']): Verbs => {
    const inputs = 'production' in getNamedInputs(root, context) ? ['production', '^production'] : ['default', '^default'];
    return Record.fromIterableWith(VERBS, (verb): [string, Readonly<Record<string, TargetConfiguration>>] => {
        const command = ['node', posix.relative(root, entry), verb].map(quoteShellArg).join(' ');
        const configured = Record.map(configurations[verb] ?? {}, (words) => ({ args: words.map(quoteShellArg).join(' ') }));
        return [
            verb,
            {
                pulumi: {
                    command,
                    options: { cwd: root },
                    dependsOn: [...(hosted || built ? ['build--*'] : []), ...(hosted && !built ? [] : ['^build']), `^${verb}`],
                    parallelism: false,
                    inputs: [...inputs, { externalDependencies: [] }],
                    ...(!Record.isEmptyRecord(configured) && { configurations: configured }),
                    metadata: { description: `Runs the Pulumi Automation API program ${hosted ? 'the project depends on' : 'of the project'} with ${command}` },
                },
            },
        ];
    });
};

const inferred = (programs: ReadonlyMap<string, string>, tracked: ReadonlySet<string>, configurations: (typeof Options.Type)['configurations'], { name, dependencies, nx }: typeof Manifest.Type, file: string, context: CreateNodesContext): Option.Option<readonly [string, CreateNodesResult]> => {
    const root = posix.dirname(file);
    const own = Option.flatMap(Option.fromUndefinedOr(name), (held) => Option.fromUndefinedOr(programs.get(held)));
    const hosting = Array.getSomes([...Record.keys(dependencies), ...(nx?.implicitDependencies ?? [])].toSorted().map((dependency) => Option.fromUndefinedOr(programs.get(dependency))));
    return Option.map(
        Option.orElse(own, () => Array.head(hosting)),
        (entry) => [file, { projects: { [root]: { root, metadata: { technologies: ['pulumi'] }, targets: aggregated(targets(root, entry, Option.isNone(own), !tracked.has(entry), context, configurations), nx?.targets ?? {}, 'pulumi') } } }] as const,
    );
};

const nodes = Effect.fnUntraced(function* (files: readonly string[], options: typeof Options.Encoded | undefined, context: CreateNodesContext) {
    const [fs, { configurations }] = yield* Effect.all([FileSystem.FileSystem, Schema.decodeEffect(Options)(options ?? {})]);
    const member = buildPackageJsonWorkspacesMatcher(buildPackageJsonPatterns(context.workspaceRoot, (file) => readJsonFile<PackageJson>(posix.join(context.workspaceRoot, file))));
    const roots = new Set(files.filter((file) => posix.basename(file) === 'project.json').map((file) => posix.dirname(file)));
    const handed = files.filter((file) => posix.basename(file) === 'package.json' && (member(file) || roots.has(posix.dirname(file))));
    const rows = Array.zip(handed, yield* Effect.forEach(handed, (file) => Effect.result(Effect.flatMap(fs.readFileString(posix.join(context.workspaceRoot, file)), Schema.decodeEffect(Manifest))), { concurrency: 'unbounded' }));
    const programs = new Map(Array.getSomes(rows.map(([file, read]) => Option.flatMap(Option.getSuccess(read), (manifest) => programmed(manifest, file)))));
    const tracked = new Set(yield* Effect.promise(() => globWithWorkspaceContext(context.workspaceRoot, [...programs.values()])));
    const results = Array.getSomes(rows.map(([file, read]) => Option.flatMap(Option.getSuccess(read), (manifest) => inferred(programs, tracked, configurations, manifest, file, context))));
    const failures = Array.filterMap(rows, ([file, read]) => Result.map(Result.flip(read), (error): [string, Error] => [file, error]));
    return yield* Array.match(failures, { onEmpty: () => Effect.succeed(results), onNonEmpty: (refused) => Effect.fail(new AggregateCreateNodesError(refused, results)) });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = [combineGlobPatterns(['**/package.json', '**/project.json']), (files, options, context) => run(nodes(files, options, context))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
