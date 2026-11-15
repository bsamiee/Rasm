import { dirname, join, relative } from 'node:path';
import { fileURLToPath } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesContext, createNodesFromFiles, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { mergeTargetConfigurations, type PackageJson, quoteShellArg } from '@nx/devkit/internal';
import { Array, Cause, Effect, FileSystem, Option, Predicate, Record, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Verbs = Readonly<Record<string, Readonly<Record<string, TargetConfiguration>>>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#package'))).name;
const PULUMI = '@pulumi/pulumi';
const VERBS = ['install', 'deploy'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Configured = Schema.optionalKey(Schema.Record(Schema.String, Schema.Array(Schema.String)));
const Options = Schema.Struct({ configurations: Schema.Struct({ install: Configured, deploy: Configured }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))) }).annotate({
    description: 'configurations: per verb, each Nx configuration of its target and the words that configuration passes the program after the verb, absent gives the targets no configuration',
});
const Dependencies = Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({})));
const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.optionalKey(Schema.String), bin: Schema.optionalKey(Schema.Union([Schema.String, Schema.Record(Schema.String, Schema.String)])), dependencies: Dependencies, devDependencies: Dependencies })).annotate({
    description: 'The name, bin, and dependencies of a package.json: a package depending on @pulumi/pulumi with one bin is a Pulumi Automation API program, and a package depending on that program runs it for itself',
});
const Input = Schema.declare((input: unknown): input is NonNullable<TargetConfiguration['inputs']>[number] => Predicate.isString(input) || Predicate.isObject(input));
const Overrides = Schema.fromJsonString(Schema.Struct({ nx: Schema.optionalKey(Schema.Struct({ targets: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ inputs: Schema.optionalKey(Schema.mutable(Schema.Array(Input))) }))) })) })).annotate({
    description: "The nx.targets rows of a project's package.json, whose inputs Nx merges over the inferred targets after every plugin",
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const run = <A, E>(effect: Effect.Effect<A, E, FileSystem.FileSystem>) => Effect.runPromise(Effect.provide(effect, layer));

const rendered = (cause: Cause.Cause<unknown>) => Effect.fail(new Cause.UnknownError(cause, Cause.pretty(cause)));

const manifested = Effect.fnUntraced(function* (workspaceRoot: string, root: string) {
    const fs = yield* FileSystem.FileSystem;
    const manifest = join(workspaceRoot, root, 'package.json');
    return (yield* fs.exists(manifest)) ? ((yield* Effect.flatMap(fs.readFileString(manifest), Schema.decodeEffect(Overrides))).nx?.targets ?? {}) : {};
});

const aggregated = (verbs: Verbs, stated: Readonly<Record<string, Pick<TargetConfiguration, 'inputs'>>>, technology: string): Record<string, TargetConfiguration> =>
    Record.fromEntries(
        Record.collect(Record.filter(verbs, Predicate.not(Record.isEmptyRecord)), (verb, tools): readonly (readonly [string, TargetConfiguration])[] => [
            [verb, { dependsOn: [`${verb}--*`], inputs: ['...', ...Array.dedupe(Record.collect(tools, (tool, target) => mergeTargetConfigurations(stated[`${verb}--${tool}`] ?? {}, { ...target, inputs: target.inputs ?? ['default', '^default'] }).inputs ?? []).flat())] }],
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
                    ...(!Record.isEmptyRecord(configured) && { configurations: configured }),
                    metadata: { description: `Runs the Pulumi Automation API program ${hosted ? 'the project depends on' : 'of the project'} with ${command}`, help: { command: `${command} --help`, example: { args: [] } } },
                },
            },
        ];
    });

const inferred = (workspaceRoot: string, programs: ReadonlyMap<string, string>, configurations: (typeof Options.Type)['configurations'], { name, dependencies }: typeof Manifest.Type, file: string) => {
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
            onNone: () => Effect.succeed({}),
            onSome: (entry) => Effect.map(manifested(workspaceRoot, root), (stated) => ({ projects: { [root]: { root, metadata: { technologies: ['pulumi'] }, targets: aggregated(targets(root, entry, Option.isNone(own), configurations), stated, 'pulumi') } } })),
        },
    );
};

const nodes = Effect.fnUntraced(function* (files: readonly string[], options: typeof Options.Encoded | undefined, context: CreateNodesContext) {
    const [fs, { configurations }] = yield* Effect.all([FileSystem.FileSystem, Schema.decodeEffect(Options)(options ?? {})]);
    const manifests = yield* Effect.forEach(files, (file) => Effect.result(Effect.flatMap(fs.readFileString(join(context.workspaceRoot, file)), Schema.decodeEffect(Manifest))), { concurrency: 'unbounded' });
    const programs = new Map(Array.getSomes(Array.zipWith(files, manifests, (file, read) => Option.flatMap(Option.getSuccess(read), (manifest) => programmed(manifest, file)))));
    const each = (file: string, index: number) =>
        Effect.fromNullishOr(manifests[index]).pipe(
            Effect.flatMap(Effect.fromResult),
            Effect.flatMap((manifest) => inferred(context.workspaceRoot, programs, configurations, manifest, file)),
            Effect.catchCause(rendered),
            run,
        );
    return yield* Effect.tryPromise({
        try: () => createNodesFromFiles((file, _options, _context, index) => each(file, index), files, options, context),
        catch: (cause) => (cause instanceof AggregateCreateNodesError ? cause : new AggregateCreateNodesError([[null, new Cause.UnknownError(cause, `${PLUGIN} infers no project`)]], [])),
    });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = ['**/package.json', (files, options, context) => run(nodes(files, options, context))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
