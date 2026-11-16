import { existsSync } from 'node:fs';
import { findPackageJSON } from 'node:module';
import { dirname, isAbsolute, join, relative, sep } from 'node:path';
import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import { type CreateNodesContext, hashArray, normalizePath, type NxJsonConfiguration, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { buildPackageJsonPatterns, buildPackageJsonWorkspacesMatcher, getNamedInputs, hashFile, hashWithWorkspaceContext, mergeTargetConfigurations, type PackageJson, type PluginCache } from '@nx/devkit/internal';
import { walkTsconfigExtendsChain } from '@nx/js/internal';
import { Array, Cause, Effect, Equal, FileSystem, identity, Option, Predicate, Record, Result, Schema, Stream, String } from 'effect';
import { ChildProcess, CommandLine } from 'effect/process';

// --- [TYPES] ---------------------------------------------------------------------------

type Verbs = Readonly<Record<string, Readonly<Record<string, TargetConfiguration>>>>;
type Stated = Readonly<Record<string, Pick<TargetConfiguration, 'inputs'>>>;
type Parse = (file: string, text: string) => { readonly module: { readonly staticImports: readonly { readonly moduleRequest: { readonly value: string }; readonly entries: readonly { readonly isType: boolean }[] }[] } };

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLATFORM = { runtime: 'node -p process.platform+process.arch' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Folder = Schema.optionalKey(Schema.String);
const Input = Schema.declare((input: unknown): input is NonNullable<TargetConfiguration['inputs']>[number] => Predicate.isString(input) || Predicate.isObject(input));
const Targets = Schema.Record(Schema.String, Schema.Struct({ inputs: Schema.optionalKey(Schema.mutable(Schema.Array(Input))) })).annotate({
    description: "The nx.targets rows of a project's package.json, whose inputs Nx merges over the inferred targets after every plugin",
});
const Overrides = Schema.fromJsonString(Schema.Struct({ nx: Schema.optionalKey(Schema.Struct({ targets: Schema.optionalKey(Targets) })) }));
const Named = Schema.fromJsonString(Schema.Struct({ name: Schema.String })).annotate({ description: 'The name field of a package.json' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const run = <A, E>(effect: Effect.Effect<A, E, NodeServices>) => Effect.runPromise(Effect.provide(effect, layer));

const fingerprinted = (folder: string) =>
    Effect.runSync(
        Effect.cached(
            Effect.map(
                FileSystem.FileSystem.use((fs) => fs.readDirectory(folder)),
                (names) => hashArray(names.toSorted().flatMap((name) => [name, ...Array.fromNullishOr(hashFile(join(folder, name)))])),
            ),
        ),
    );

const spawned = Effect.fnUntraced(function* (argv: Array.NonEmptyReadonlyArray<string>, options: ChildProcess.CommandOptions = {}) {
    const handle = yield* ChildProcess.make(Array.headNonEmpty(argv), Array.tailNonEmpty(argv), { stdin: 'ignore', ...options, extendEnv: true, detached: false });
    const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? stdout : yield* new Cause.UnknownError(code, [`${CommandLine.display(argv)} exits ${code}`, ...[stderr.trim()].filter(String.isNonEmpty)].join(': '));
}, Effect.scoped);

const within = (at: string) => !(at.startsWith('..') || at.split('/').includes('node_modules'));

const tokened = (root: string) => (at: string) => (relative(root, at).startsWith('..') ? join('{workspaceRoot}', at) : join('{projectRoot}', relative(root, at)));

const namedInput = (root: string, context: CreateNodesContext) => ('production' in getNamedInputs(root, context) ? 'production' : 'default');

const tagged = ({ release }: NxJsonConfiguration) => Array.dedupe([...Array.ensure(release?.projects ?? []), ...Object.values(release?.groups ?? {}).flatMap(({ projects }) => Array.ensure(projects))].filter((selector) => selector.startsWith('tag:')).map((selector) => selector.slice('tag:'.length)));

const anchored = (workspaceRoot: string) => {
    const member = buildPackageJsonWorkspacesMatcher(buildPackageJsonPatterns(workspaceRoot, (file: string) => readJsonFile<PackageJson>(join(workspaceRoot, file))));
    return (root: string) => existsSync(join(workspaceRoot, root, 'project.json')) || (member(join(root, 'package.json')) && existsSync(join(workspaceRoot, root, 'package.json')));
};

const above = (folder: string): readonly string[] =>
    Array.unfold(
        Option.some(folder),
        Option.map((at: string) => [at, Option.liftPredicate(dirname(at), () => at !== '.')] as const),
    );

const extended = (workspaceRoot: string, root: string): readonly string[] => {
    const files: string[] = [];
    Option.toArray(
        Array.findFirst(
            above(root).map((at) => join(workspaceRoot, at, 'tsconfig.json')),
            existsSync,
        ),
    ).forEach((config) =>
        walkTsconfigExtendsChain(config, (at) => {
            files.push(at);
            return 'continue';
        }),
    );
    return files.map((at) => normalizePath(relative(workspaceRoot, at))).filter(within);
};

const stamped = (workspaceRoot: string) => (sources: readonly string[]) => Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...sources]));

const recalled = <A extends { readonly sources: readonly string[]; readonly stamp: string }>(cache: PluginCache<A>, key: string, stamp: (sources: readonly string[]) => Effect.Effect<string>) =>
    Effect.findFirst(Array.fromNullishOr(cache.get(key)), ({ sources, stamp: held }) => Effect.map(stamp(sources), Equal.equals(held)));

const manifested = Effect.fnUntraced(function* (workspaceRoot: string, root: string) {
    const fs = yield* FileSystem.FileSystem;
    const manifest = join(workspaceRoot, root, 'package.json');
    return (yield* fs.exists(manifest)) ? ((yield* Effect.flatMap(fs.readFileString(manifest), Schema.decodeEffect(Overrides))).nx?.targets ?? {}) : {};
});

const parsed = Effect.fnUntraced(function* (parse: Parse, workspaceRoot: string, files: readonly string[]) {
    const texts = yield* Effect.forEach(files, (at) => FileSystem.FileSystem.use((fs) => fs.readFileString(join(workspaceRoot, at))), { concurrency: 'unbounded' });
    const statements = Array.zipWith(files, texts, (at, text) => parse(at, text).module.staticImports.map((statement) => [statement, join(workspaceRoot, at)] as const)).flat();
    return statements.filter(([{ entries }]) => entries.length === 0 || entries.some(({ isType }) => !isType)).map(([{ moduleRequest }, from]) => [moduleRequest.value, from] as const);
});

const packaged = Effect.fnUntraced(function* (imports: readonly (readonly [specifier: string, from: string])[]) {
    const fs = yield* FileSystem.FileSystem;
    const found = Array.getSomes(imports.filter(([specifier]) => !(specifier.startsWith('.') || specifier.startsWith('#') || isAbsolute(specifier) || URL.canParse(specifier))).map(([specifier, from]) => Option.flatMap(Option.liftThrowable(findPackageJSON)(specifier, from), Option.fromUndefinedOr)));
    const manifests = yield* Effect.forEach(Array.dedupe(found), (at) => fs.realPath(at), { concurrency: 'unbounded' });
    const read = yield* Effect.forEach(Array.dedupe(manifests), (at) => Effect.map(Effect.flatMap(fs.readFileString(at), Schema.decodeEffect(Named)), ({ name }) => (at.split(sep).includes('node_modules') ? Result.succeed(name) : Result.fail(name))), { concurrency: 'unbounded' });
    const [packages, linked] = Array.partition(read, identity);
    return { packages: Array.dedupe(packages), linked: Array.dedupe(linked) };
});

const aggregated = (verbs: Verbs, stated: Stated, technology: string): Record<string, TargetConfiguration> =>
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
                                .map((entry) => (Predicate.isObject(entry) && 'dependentTasksOutputFiles' in entry ? { ...entry, transitive: true } : entry)),
                        ),
                    ],
                },
            ],
            ...Record.collect(tools, (tool, target): readonly [string, TargetConfiguration] => [`${verb}--${tool}`, { ...target, metadata: { technologies: [technology], ...target.metadata } }]),
        ]).flat(),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { above, aggregated, anchored, extended, fingerprinted, Folder, Input, manifested, namedInput, packaged, parsed, PLATFORM, recalled, run, spawned, type Stated, stamped, tagged, Targets, tokened, type Verbs, within };
