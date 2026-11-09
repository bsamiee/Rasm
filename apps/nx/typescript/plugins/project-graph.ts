import { createRequire, findPackageJSON } from 'node:module';
import { pathToFileURL } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesResultArray, detectPackageManager, hashArray, type ProjectConfiguration, type TargetConfiguration } from '@nx/devkit';
import { getLockFileName, globWithWorkspaceContext, hashObject, hashWithWorkspaceContext, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Option, Path, Record, Result, Schema } from 'effect';
import { API, type ParsedCommandLine } from 'typescript/sync';
import { PACKAGES } from '#/plugins/adobe-packages';

// --- [TYPES] ---------------------------------------------------------------------------

type Targets = Readonly<Record<string, TargetConfiguration>>;
type Program = Effect.Success<ReturnType<typeof programs>>[number];
type Kind = keyof typeof MARKERS | keyof typeof PACKAGES;

// --- [CONSTANTS] -----------------------------------------------------------------------

const CONFIG = 'tsconfig.json';
const MARKERS = { storybook: '.storybook/main.{js,ts,jsx,tsx,mjs,mts,cjs,cts}', cloudflare: 'cloudflare.config.ts', vite: 'vite.config.{js,mjs,ts,cjs,mts,cts}' } as const;
const VERBS = ['build', 'check', 'fix', 'upgrade'];
const TECHNOLOGIES = ['typescript'];
const RELEASE = 'release';

// --- [MODELS] --------------------------------------------------------------------------

const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.String })).annotate({ description: 'The package.json closest to this module, its own package in the tree and in dist alike' });
const Options = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String) }).annotate({ description: 'Plugin options from the nx.json row' });
const Built = Schema.Struct({ outDir: Schema.String });
const Vite = Schema.Struct({ build: Built, environments: Schema.Record(Schema.String, Schema.Struct({ build: Built })) }).annotate({ description: 'The resolved Vite config fields naming every folder vite build writes' });

// --- [ERRORS] --------------------------------------------------------------------------

class TypeScriptError extends Schema.TaggedError<TypeScriptError, { readonly brand: unique symbol }>()('TypeScriptError', { message: Schema.String, cause: Schema.optionalKey(Schema.Defect()) }) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const line = (argv: readonly string[]) => argv.map(quoteShellArg).join(' ');

const programs = Effect.fnUntraced(function* (workspaceRoot: string, roots: readonly string[]) {
    const path = yield* Path.Path;
    const relative = (at: string) => path.relative(workspaceRoot, at);
    const referenced = ({ projectReferences = [] }: ParsedCommandLine) => projectReferences.map(({ path: at }) => (path.extname(at) === '.json' ? at : path.join(at, CONFIG)));
    const failed = (cause: unknown) => TypeScriptError.make({ message: `Reading ${roots.length} TypeScript configurations: ${cause instanceof Error ? cause.message : String(cause)}`, cause });
    const api = yield* Effect.acquireRelease(
        Effect.sync(() => new API({ cwd: workspaceRoot })),
        (opened) => Effect.sync(() => opened.close()),
    );
    const solutions = yield* Effect.try({ try: () => roots.map((root) => ({ root, config: path.join(workspaceRoot, root, CONFIG) })).map(({ root, config }) => ({ root, configs: [config, ...referenced(api.parseConfigFile(config))] })), catch: failed });
    const loaded = yield* Effect.try({
        try: () => {
            const configs = solutions.flatMap((solution) => solution.configs);
            const snapshot = api.createSnapshot({ openProjects: configs });
            const projects = configs.map((config) => snapshot.getConfiguredProject(config));
            return Record.fromEntries(Array.zipWith(configs, projects, (config, project) => [config, project === undefined ? Option.none() : Option.some({ config, parsed: project.parsedCommandLine, chain: project.program.getConfigFileNames() })] as const));
        },
        catch: failed,
    });
    return solutions.map(({ root, configs }) => {
        const present = Array.getSomes(configs.map((config) => Option.flatten(Record.get(loaded, config))));
        const holding = present.filter(({ parsed }) => parsed.fileNames.length > 0);
        const checked = Option.match(
            Array.findFirst(holding, ({ config }) => config === configs[0]),
            { onSome: Array.of, onNone: () => holding },
        );
        return {
            root,
            commands: checked.map(({ config, parsed }) => ['tsc', referenced(parsed).length > 0 ? '--build' : '--project', path.relative(path.join(workspaceRoot, root), config)]),
            outputs: Array.dedupe(Array.getSomes(holding.filter(({ parsed }) => parsed.options.noEmit !== true).flatMap(({ parsed: { options } }) => [options.outDir, options.declarationDir].map(Option.fromUndefinedOr))).map(relative)),
            chain: Array.dedupe(present.flatMap(({ chain }) => chain.map(relative))).filter((at) => !at.startsWith(`${root}/`)),
        };
    });
}, Effect.scoped);

const bundled = Effect.fnUntraced(function* (workspaceRoot: string, configs: readonly { readonly root: string; readonly file: string }[]) {
    const path = yield* Path.Path;
    const results = yield* Effect.forEach(
        configs,
        Effect.fnUntraced(function* ({ root, file }) {
            const folder = path.join(workspaceRoot, root);
            const vite: { readonly resolveConfig: (config: object, command: string, mode: string, environment: string) => Promise<unknown> } = yield* Effect.tryPromise({
                try: () => import(pathToFileURL(createRequire(path.join(folder, 'package.json')).resolve('vite')).href),
                catch: (cause) => TypeScriptError.make({ message: `${file}: vite does not resolve from ${root}`, cause }),
            });
            const { build, environments } = yield* Effect.tryPromise({ try: () => vite.resolveConfig({ configFile: path.join(workspaceRoot, file), root: folder, logLevel: 'silent' }, 'build', 'production', 'production'), catch: (cause) => TypeScriptError.make({ message: `${file}: ${String(cause)}`, cause }) }).pipe(
                Effect.flatMap(Schema.decodeUnknownEffect(Vite)),
            );
            const written = [build.outDir, ...Record.values(environments).map((environment) => environment.build.outDir)];
            return Array.dedupe(written.map((at) => path.relative(workspaceRoot, path.resolve(folder, at))));
        }, Effect.result),
        { concurrency: 'unbounded' },
    );
    return {
        outputs: Record.fromEntries(Array.zip(configs, results).flatMap(([{ root }, result]) => (Result.isSuccess(result) ? [[root, result.success] as const] : []))),
        failures: Array.zip(configs, results).flatMap(([{ file }, result]) => (Result.isFailure(result) ? [[file, new Error(result.failure.message)] satisfies [string, Error]] : [])),
    };
});

const targets = (path: Path.Path, name: string, program: Program, held: ReadonlySet<Kind>, bundles: Option.Option<readonly string[]>, external: readonly string[], outputPath: Option.Option<string>): Targets => {
    const metadata = (description: string, help: readonly string[], example: readonly string[]) => ({ technologies: TECHNOLOGIES, description, help: { command: line([...help, '--help']), example: { args: [...example] } } });
    const command = (argv: readonly string[]) => ({ executor: 'nx:run-commands', options: { command: line(argv), cwd: program.root } });
    const chained = program.chain.map((at) => path.join('{workspaceRoot}', at));
    const compiled = (held.has('cloudflare') ? [{ dependentTasksOutputFiles: '**/*.d.ts' }] : []) satisfies NonNullable<TargetConfiguration['inputs']>;
    const tsc: TargetConfiguration = {
        executor: 'nx:run-commands',
        options: { commands: program.commands.map(line), cwd: program.root },
        cache: true,
        inputs: ['default', '^default', ...chained, ...compiled, { externalDependencies: ['typescript', ...external] }],
        ...(held.has('cloudflare') ? { dependsOn: ['build--cf'] } : {}),
        outputs: [],
    };
    const storybook = Option.map(outputPath, (at) => path.join(at, 'storybook', '{projectName}'));
    const inferred: Targets = {
        ...(program.outputs.length > 0
            ? {
                  'build--tsc': { ...tsc, outputs: program.outputs.map((at) => path.join('{workspaceRoot}', at)), metadata: metadata('Emits JavaScript and declarations with tsc, type-checking every program it builds', ['tsc'], ['--listEmittedFiles']) },
                  'check--tsc': { executor: 'nx:noop', dependsOn: ['build--tsc'], inputs: tsc.inputs ?? [], metadata: metadata('Type-checks through the build--tsc compile', ['tsc'], ['--extendedDiagnostics']) },
              }
            : { 'check--tsc': { ...tsc, metadata: metadata('Type-checks every program of the project with tsc', ['tsc'], ['--extendedDiagnostics']) } }),
        ...(held.has('cloudflare')
            ? {
                  'build--cf': {
                      ...command(['cf', 'workers', 'types']),
                      cache: true,
                      inputs: ['default', '^default', { externalDependencies: ['cf'] }],
                      outputs: ['{projectRoot}/.cloudflare/types'],
                      metadata: metadata('Generates the Worker binding and runtime types from cloudflare.config.ts', ['cf', 'workers', 'types'], ['--mode', 'production']),
                  },
              }
            : {}),
        ...(held.has('storybook')
            ? {
                  'build--storybook': {
                      ...command(['storybook', 'build', ...Option.match(storybook, { onNone: () => [], onSome: (at) => ['--output-dir', path.relative(program.root, at)] })]),
                      cache: true,
                      inputs: ['default', '^default', ...chained, { externalDependencies: ['storybook'] }],
                      outputs: [Option.match(storybook, { onNone: () => '{projectRoot}/storybook-static', onSome: (at) => path.join('{workspaceRoot}', at) })],
                      metadata: metadata('Builds the static Storybook of the project stories', ['storybook', 'build'], ['--test']),
                  },
              }
            : {}),
        ...Option.match(bundles, {
            onNone: (): Targets => ({}),
            onSome: (folders) => ({
                'build--vite': {
                    ...command(['vite', 'build']),
                    cache: true,
                    inputs: ['default', '^default', ...chained, { externalDependencies: ['vite'] }],
                    outputs: folders.map((at) => path.join('{workspaceRoot}', at)),
                    metadata: metadata('Builds every environment of the Vite config with vite build', ['vite', 'build'], ['--mode', 'production']),
                },
            }),
        }),
        ...Record.mapEntries(
            Record.filter(PACKAGES, (_, kind) => held.has(kind)),
            ({ archive, variables, description }, kind): [string, TargetConfiguration] => {
                const destination = { outputPath: Option.match(outputPath, { onNone: () => program.root, onSome: (at) => path.join(at, kind, '{projectName}') }) };
                return [
                    `build--${kind}`,
                    {
                        executor: `${name}:package`,
                        dependsOn: ['build--*'],
                        cache: true,
                        inputs: [{ dependentTasksOutputFiles: '**/*' }, ...variables.map((env) => ({ env })), { externalDependencies: [] }],
                        outputs: [`{options.outputPath}/*${archive}`],
                        ...(variables.length === 0 ? { options: destination } : { options: {}, configurations: { release: destination } }),
                        metadata: { technologies: TECHNOLOGIES, description },
                    },
                ];
            },
        ),
    };
    return {
        ...inferred,
        ...Record.fromIterableWith(
            VERBS.filter((verb) => Record.keys(inferred).some((target) => target.startsWith(`${verb}--`))),
            (verb): readonly [string, TargetConfiguration] => {
                const inputs = Record.collect(
                    Record.filter(inferred, (_, target) => target.startsWith(`${verb}--`)),
                    (_, target) => target.inputs ?? [],
                ).flat();
                return [
                    verb,
                    {
                        executor: 'nx:noop',
                        dependsOn: [`${verb}--*`],
                        inputs: ['...', ...Array.dedupe(inputs)],
                        metadata: { technologies: TECHNOLOGIES, description: `Runs every ${verb}--* target` },
                    },
                ];
            },
        ),
    };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = [
    `**/${CONFIG}`,
    (files, options, context) =>
        Effect.runPromise(
            Effect.gen(function* () {
                const [path, fs, decoded] = yield* Effect.all([Path.Path, FileSystem.FileSystem, Schema.decodeEffect(Options)(options ?? {})]);
                const { workspaceRoot } = context;
                const roots = Array.dedupe(files.map(path.dirname)).toSorted();
                const projects = yield* Effect.filter(roots, (root) => fs.exists(path.join(workspaceRoot, root, 'package.json')), { concurrency: 'unbounded' });
                const kinds = [
                    ...Record.collect(MARKERS, (kind, pattern) => ({ kind, pattern, accepted: (_: string) => Effect.succeed(true) })),
                    ...Record.collect(PACKAGES, (kind, { manifest, accepts }) => ({ kind, pattern: manifest, accepted: (file: string) => Effect.flatMap(fs.readFileString(path.join(workspaceRoot, file)), accepts) })),
                ];
                const markers = yield* Effect.forEach(
                    kinds,
                    ({ kind, pattern, accepted }) =>
                        Effect.promise(() => globWithWorkspaceContext(workspaceRoot, [`**/${pattern}`])).pipe(
                            Effect.flatMap((found) => Effect.filter(found, accepted, { concurrency: 'unbounded' })),
                            Effect.map(Array.map((file) => ({ root: path.join(file, ...Array.replicate('..', pattern.split('/').length)), kind, file }))),
                        ),
                    { concurrency: 'unbounded' },
                ).pipe(Effect.map(Array.flatten));
                const configs = markers.filter(({ root, kind }) => kind === 'vite' && projects.includes(root));
                const held = (root: string) => new Set(markers.flatMap((marker) => (marker.root === root ? [marker.kind] : [])));
                const external = Option.isSome(Option.liftThrowable(() => createRequire(path.join(workspaceRoot, 'package.json')).resolve('@effect/tsgo/package.json'))()) ? ['@effect/tsgo'] : [];
                const modules = (yield* fs.readDirectory(import.meta.dirname)).toSorted().map((module) => path.join(import.meta.dirname, module));
                const hash = hashArray([
                    yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, ['**/tsconfig*.json', getLockFileName(detectPackageManager(workspaceRoot)), ...configs.map(({ file }) => file)])),
                    ...(yield* Effect.forEach(modules, (module) => fs.readFileString(module))),
                    hashObject(decoded),
                    ...projects,
                ]);
                const { name } = yield* Effect.fromNullishOr(findPackageJSON(import.meta.url)).pipe(Effect.flatMap(fs.readFileString), Effect.flatMap(Schema.decodeEffect(Manifest)));
                const cache = new PluginCache<{ readonly programs: readonly Program[]; readonly bundles: Readonly<Record<string, readonly string[]>> }>(path.join(workspaceDataDirectory, `${path.basename(name)}-${hashObject(decoded)}.hash`));
                const { read, failures } = yield* Option.match(Option.fromNullishOr(cache.get(hash)), {
                    onSome: (entry) => Effect.succeed({ read: entry, failures: Array.empty<[string, Error]>() }),
                    onNone: () =>
                        Effect.tap(
                            Effect.map(Effect.all([programs(workspaceRoot, projects), bundled(workspaceRoot, configs)], { concurrency: 'unbounded' }), ([found, { outputs, failures: failed }]) => ({ read: { programs: found, bundles: outputs }, failures: failed })),
                            (fresh) =>
                                fresh.failures.length === 0
                                    ? Effect.sync(() => {
                                          cache.set(hash, fresh.read);
                                          cache.writeToDisk();
                                      })
                                    : Effect.void,
                        ),
                });
                const released = (root: string): Pick<ProjectConfiguration, 'release' | 'tags'> =>
                    Option.match(
                        Array.findFirst(
                            Record.collect(PACKAGES, (kind, { manifest }) => ({ kind, manifest })),
                            ({ kind }) => held(root).has(kind),
                        ),
                        {
                            onNone: () => ({}),
                            onSome: ({ manifest }) => ({ tags: [RELEASE], release: { version: { versionActions: `${name}/release/version-actions`, versionActionsOptions: { manifest } } } }),
                        },
                    );
                const nodes = read.programs.map((program): CreateNodesResultArray[number] => [
                    path.join(program.root, CONFIG),
                    { projects: { [program.root]: { metadata: { technologies: TECHNOLOGIES }, targets: targets(path, name, program, held(program.root), Record.get(read.bundles, program.root), external, Option.fromUndefinedOr(decoded.outputPath)), ...released(program.root) } } },
                ]);
                return failures.length === 0 ? nodes : yield* Effect.fail(new AggregateCreateNodesError(failures, nodes));
            }).pipe(
                Effect.mapError((error) => (error instanceof AggregateCreateNodesError ? error : new AggregateCreateNodesError([[null, new Error(error.message)]], []))),
                Effect.provide(layer),
            ),
        ),
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes, TypeScriptError };
