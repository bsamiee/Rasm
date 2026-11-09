import { createRequire, findPackageJSON, registerHooks } from 'node:module';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesResultArray, detectPackageManager, hashArray, type ProjectConfiguration, type TargetConfiguration } from '@nx/devkit';
import { getLockFileName, globWithWorkspaceContext, hashObject, hashWithWorkspaceContext, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Option, Path, Record, Result, Schema } from 'effect';
import { API, type ParsedCommandLine } from 'typescript/sync';
import { PACKAGES, TypeScriptError } from '#/plugins/adobe-packages';

// --- [TYPES] ---------------------------------------------------------------------------

type Targets = Readonly<Record<string, TargetConfiguration>>;
type Program = Effect.Success<ReturnType<typeof programs>>[number];
type Kind = keyof typeof MARKERS | keyof typeof PACKAGES | 'publint';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CONFIG = 'tsconfig.json';
const MARKERS = { storybook: '.storybook/main.{js,ts,jsx,tsx,mjs,mts,cjs,cts}', cloudflare: 'cloudflare.config.ts', vite: 'vite.config.{js,mjs,ts,cjs,mts,cts}' } as const;
const TECHNOLOGIES = ['typescript'];

// --- [MODELS] --------------------------------------------------------------------------

const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.String })).annotate({ description: 'The package.json closest to this module, its own package in the tree and in dist alike' });
const Package = Schema.fromJsonString(Schema.Struct({ private: Schema.optionalKey(Schema.Boolean) })).annotate({ description: 'A workspace package.json field deciding whether its package publishes, the rule Nx core applies to nx-release-publish' });
const Options = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String) }).annotate({ description: 'Plugin options from the nx.json row' });
const Built = Schema.Struct({ outDir: Schema.String });
const Vite = Schema.Struct({ build: Built, environments: Schema.Record(Schema.String, Schema.Struct({ build: Built })) }).annotate({ description: 'The resolved Vite config fields naming every folder vite build writes' });

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
            outputs: Array.dedupe(
                holding
                    .filter(({ parsed }) => parsed.options.noEmit !== true)
                    .flatMap(
                        ({ parsed: { options } }) =>
                            [
                                [options.outDir, `**/*.{js,cjs,mjs,jsx${options.resolveJsonModule === true ? ',json' : ''},d.ts,d.cts,d.mts}{,.map}`],
                                [options.declarationDir, '**/*.{d.ts,d.cts,d.mts}{,.map}'],
                            ] as const,
                    )
                    .flatMap(([at, emitted]) => (at === undefined ? [] : [path.join(relative(at), emitted)])),
            ),
            chain: Array.dedupe(present.flatMap(({ chain }) => chain.map(relative))).filter((at) => !at.startsWith(`${root}/`)),
        };
    });
}, Effect.scoped);

const bundled = Effect.fnUntraced(function* (workspaceRoot: string, configs: readonly { readonly root: string; readonly file: string }[]) {
    const path = yield* Path.Path;
    const urls = new Set<string>();
    yield* Effect.acquireRelease(
        Effect.sync(() =>
            registerHooks({
                resolve: (specifier, context, next) => {
                    const resolved = next(specifier, context);
                    urls.add(resolved.url);
                    return resolved;
                },
            }),
        ),
        (hooks) => Effect.sync(() => hooks.deregister()),
    );
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
        loaded: yield* Effect.promise(() =>
            globWithWorkspaceContext(
                workspaceRoot,
                Array.getSomes([...urls].map((url) => Option.liftThrowable(fileURLToPath)(url))).map((at) => path.relative(workspaceRoot, at)),
            ),
        ),
    };
}, Effect.scoped);

const targets = (path: Path.Path, plugin: { readonly name: string; readonly inputs: NonNullable<TargetConfiguration['inputs']> }, program: Program, held: ReadonlySet<Kind>, bundles: Option.Option<readonly string[]>, external: readonly string[], outputPath: Option.Option<string>): Targets => {
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
        ...(held.has('publint')
            ? {
                  'check--publint': {
                      ...command(['publint', '--strict']),
                      dependsOn: ['build--*'],
                      cache: true,
                      inputs: ['default', { dependentTasksOutputFiles: '**/*' }, { externalDependencies: ['publint'] }],
                      outputs: [],
                      metadata: metadata('Lints the tarball the package manager packs against its package.json with publint, warnings failing through --strict', ['publint', 'run'], ['--level', 'warning']),
                  },
              }
            : {}),
        ...Record.mapEntries(
            Record.filter(PACKAGES, (_, kind) => held.has(kind)),
            ({ archive, variables, description }, kind): [string, TargetConfiguration] => {
                const destination = { outputPath: Option.match(outputPath, { onNone: () => program.root, onSome: (at) => path.join(at, kind, '{projectName}') }) };
                const built = Option.isSome(bundles) || program.outputs.length > 0;
                return [
                    `build--${kind}`,
                    {
                        executor: `${plugin.name}:package`,
                        ...(built ? { dependsOn: ['build--*'] } : {}),
                        cache: true,
                        inputs: [built ? { dependentTasksOutputFiles: '**/*' } : 'default', ...variables.map((env) => ({ env })), ...plugin.inputs],
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
        ...Record.map(
            Array.groupBy(Record.toEntries(inferred), ([target]) => target.slice(0, target.indexOf('--'))),
            (members, verb): TargetConfiguration => ({
                executor: 'nx:noop',
                dependsOn: [`${verb}--*`],
                inputs: ['...', ...Array.dedupe(members.flatMap(([, target]) => target.inputs ?? []))],
                metadata: { technologies: TECHNOLOGIES, description: `Runs every ${verb}--* target` },
            }),
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
                const installed = (specifier: string) => Option.isSome(Option.liftThrowable(findPackageJSON)(specifier, pathToFileURL(path.join(workspaceRoot, 'package.json'))));
                const published = installed('publint')
                    ? yield* Effect.filter(
                          projects,
                          (root) =>
                              fs.readFileString(path.join(workspaceRoot, root, 'package.json')).pipe(
                                  Effect.flatMap(Schema.decodeEffect(Package)),
                                  Effect.map((manifest) => manifest.private !== true),
                                  Effect.orElseSucceed(() => false),
                              ),
                          { concurrency: 'unbounded' },
                      )
                    : [];
                const held = (root: string) => new Set<Kind>([...markers.flatMap((marker) => (marker.root === root ? [marker.kind] : [])), ...(published.includes(root) ? ['publint' as const] : [])]);
                const external = installed('@effect/tsgo') ? ['@effect/tsgo'] : [];
                const modules = (yield* fs.readDirectory(import.meta.dirname)).toSorted().map((module) => path.join(import.meta.dirname, module));
                const hash = hashArray([
                    yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, ['**/tsconfig*.json', getLockFileName(detectPackageManager(workspaceRoot)), ...configs.map(({ file }) => file)])),
                    ...(yield* Effect.forEach(modules, (module) => fs.readFileString(module))),
                    hashObject(decoded),
                    ...projects,
                ]);
                const located = yield* Effect.fromNullishOr(findPackageJSON(import.meta.url));
                const { name } = yield* Effect.flatMap(fs.readFileString(located), Schema.decodeEffect(Manifest));
                const tracked = yield* Effect.promise(() => globWithWorkspaceContext(workspaceRoot, [path.relative(workspaceRoot, located)]));
                const plugin = { name, inputs: tracked.length === 0 ? [{ externalDependencies: [name] }] : [{ input: 'default', projects: [name] }, { externalDependencies: [] }] };
                const digest = (sources: readonly string[]) => Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...sources]));
                const cache = new PluginCache<{ readonly programs: readonly Program[]; readonly bundles: Readonly<Record<string, readonly string[]>>; readonly loaded: readonly string[]; readonly digest: string }>(path.join(workspaceDataDirectory, `${path.basename(name)}-${hashObject(decoded)}.hash`));
                const valid = yield* Effect.filter(Option.toArray(Option.fromNullishOr(cache.get(hash))), (entry) => Effect.map(digest(entry.loaded), (current) => current === entry.digest));
                const { read, failures } = yield* Option.match(Array.head(valid), {
                    onSome: (entry) => Effect.succeed({ read: entry, failures: Array.empty<[string, Error]>() }),
                    onNone: () =>
                        Effect.all([programs(workspaceRoot, projects), bundled(workspaceRoot, configs)], { concurrency: 'unbounded' }).pipe(
                            Effect.flatMap(([found, { outputs, failures: failed, loaded }]) => Effect.map(digest(loaded), (current) => ({ read: { programs: found, bundles: outputs, loaded, digest: current }, failures: failed }))),
                            Effect.tap((fresh) =>
                                fresh.failures.length === 0
                                    ? Effect.sync(() => {
                                          cache.set(hash, fresh.read);
                                          cache.writeToDisk();
                                      })
                                    : Effect.void,
                            ),
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
                            onSome: ({ manifest }) => ({ tags: ['release'], release: { version: { versionActions: `${plugin.name}/release/version-actions`, versionActionsOptions: { manifest } } } }),
                        },
                    );
                const nodes = read.programs.map((program): CreateNodesResultArray[number] => [
                    path.join(program.root, CONFIG),
                    { projects: { [program.root]: { metadata: { technologies: TECHNOLOGIES }, targets: targets(path, plugin, program, held(program.root), Record.get(read.bundles, program.root), external, Option.fromUndefinedOr(decoded.outputPath)), ...released(program.root) } } },
                ]);
                return failures.length === 0 ? nodes : yield* Effect.fail(new AggregateCreateNodesError(failures, nodes));
            }).pipe(
                Effect.mapError((error) => (error instanceof AggregateCreateNodesError ? error : new AggregateCreateNodesError([[null, new Error(error.message)]], []))),
                Effect.provide(layer),
            ),
        ),
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
