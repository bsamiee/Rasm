import { dirname, join, relative } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesContext, type CreateNodesResult, type TargetConfiguration } from '@nx/devkit';
import { mergeTargetConfigurations, quoteShellArg } from '@nx/devkit/internal';
import { Array, Cause, Effect, FileSystem, Option, Predicate, Record, Result, Schema } from 'effect';

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
        nx: Schema.optionalKey(Schema.Struct({ targets: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ inputs: Schema.optionalKey(Schema.mutable(Schema.Array(Input))) }))) })),
    }),
).annotate({
    description: 'The name, bin, dependencies, and nx.targets rows of a package.json: a package depending on @pulumi/pulumi with one bin is a Pulumi Automation API program, a package depending on that program runs it for itself, and Nx merges nx.targets inputs over the inferred targets after every plugin',
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
                (entry) => [program, join(dirname(file), entry)] as const,
            ),
    );

const targets = (root: string, entry: string, hosted: boolean, configurations: (typeof Options.Type)['configurations']): Verbs =>
    Record.fromIterableWith(VERBS, (verb): [string, Readonly<Record<string, TargetConfiguration>>] => {
        const command = ['node', relative(root, entry), verb].map(quoteShellArg).join(' ');
        const configured = Record.map(configurations[verb] ?? {}, (words) => ({ args: words.map(quoteShellArg).join(' ') }));
        return [
            verb,
            {
                pulumi: {
                    command,
                    options: { cwd: root },
                    dependsOn: [hosted ? 'build--*' : '^build', `^${verb}`],
                    parallelism: false,
                    inputs: ['default', '^default', { externalDependencies: [] }],
                    ...(!Record.isEmptyRecord(configured) && { configurations: configured }),
                    metadata: { description: `Runs the Pulumi Automation API program ${hosted ? 'the project depends on' : 'of the project'} with ${command}` },
                },
            },
        ];
    });

const inferred = (programs: ReadonlyMap<string, string>, configurations: (typeof Options.Type)['configurations'], { name, dependencies, nx }: typeof Manifest.Type, file: string): CreateNodesResult => {
    const root = dirname(file);
    const own = Option.flatMap(Option.fromUndefinedOr(name), (held) => Option.fromUndefinedOr(programs.get(held)));
    const hosting = Array.getSomes(
        Record.keys(dependencies)
            .toSorted()
            .map((dependency) => Option.fromUndefinedOr(programs.get(dependency))),
    );
    return Option.match(
        Option.orElse(own, () => Array.head(hosting)),
        {
            onNone: () => ({}),
            onSome: (entry) => ({ projects: { [root]: { root, metadata: { technologies: ['pulumi'] }, targets: aggregated(targets(root, entry, Option.isNone(own), configurations), nx?.targets ?? {}, 'pulumi') } } }),
        },
    );
};

const nodes = Effect.fnUntraced(function* (files: readonly string[], options: typeof Options.Encoded | undefined, context: CreateNodesContext) {
    const [fs, { configurations }] = yield* Effect.all([FileSystem.FileSystem, Schema.decodeEffect(Options)(options ?? {})]);
    const rows = Array.zip(files, yield* Effect.forEach(files, (file) => Effect.result(Effect.flatMap(fs.readFileString(join(context.workspaceRoot, file)), Schema.decodeEffect(Manifest))), { concurrency: 'unbounded' }));
    const programs = new Map(Array.getSomes(rows.map(([file, read]) => Option.flatMap(Option.getSuccess(read), (manifest) => programmed(manifest, file)))));
    const results = Array.filterMap(rows, ([file, read]) => Result.map(read, (manifest) => [file, inferred(programs, configurations, manifest, file)] as const));
    const failures = Array.filterMap(rows, ([file, read]) => Result.map(Result.flip(read), (error): [string, Error] => [file, new Cause.UnknownError(error, `${file} does not decode as a package manifest: ${error.message}`)]));
    return yield* Array.match(failures, { onEmpty: () => Effect.succeed(results), onNonEmpty: (refused) => Effect.fail(new AggregateCreateNodesError(refused, results)) });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = ['**/package.json', (files, options, context) => run(nodes(files, options, context))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
