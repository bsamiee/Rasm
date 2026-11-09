import { createRequire, findPackageJSON } from 'node:module';
import process from 'node:process';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesResultArray, detectPackageManager, hashArray, type ProjectConfiguration, type TargetConfiguration } from '@nx/devkit';
import { getLockFileName, globWithWorkspaceContext, hashObject, hashWithWorkspaceContext, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { API, type ParsedCommandLine } from '@typescript/native/sync';
import { Array, Effect, FileSystem, Option, Path, Record, Result, Schema, Stream } from 'effect';
import { ChildProcess } from 'effect/process';
import { PACKAGES, TypeScriptError } from '#/plugins/adobe-packages';
import { type Configured, Report } from '#/plugins/vite-config';

// --- [TYPES] ---------------------------------------------------------------------------

type Program = Effect.Success<ReturnType<typeof programs>>[number];
type Kind = keyof typeof MARKERS | keyof typeof PACKAGES | 'publint';
type Runner = keyof typeof RUNNERS;
interface Read {
    readonly root: string;
    readonly file: string;
    readonly kind: keyof typeof READS;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const CONFIG = 'tsconfig.json';
const MARKERS = {
    storybook: '.storybook/main.{js,ts,jsx,tsx,mjs,mts,cjs,cts}',
    cloudflare: 'cloudflare.config.ts',
    vite: 'vite.config.{js,mjs,ts,cjs,mts,cts}',
    vitest: '{vitest,vite}.config.{ts,mts,cts,js,mjs,cjs}',
    playwright: 'playwright.config.{ts,js,mts,mjs,cts,cjs}',
} as const;
const TECHNOLOGIES = ['typescript'];

// --- [MODELS] --------------------------------------------------------------------------

const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.String })).annotate({ description: 'The package.json closest to this module, its own package in the tree and in dist alike' });
const Options = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String) }).annotate({ description: 'Plugin options from the nx.json row' });
const Bin = Schema.fromJsonString(Schema.Struct({ bin: Schema.Record(Schema.String, Schema.String) })).annotate({ description: 'The package.json of a test runner, naming its command scripts' });
const Listing = Schema.fromJsonString(Schema.Struct({ config: Schema.Struct({ projects: Schema.Array(Schema.Struct({ outputDir: Schema.String })) }), suites: Schema.Array(Schema.Unknown) })).annotate({
    description: 'playwright test --list --reporter=json: every project output folder and the suites holding a test',
});

// --- [TABLES] --------------------------------------------------------------------------

const READS = {
    ...Record.map({ vite: 'vite', vitest: 'vitest' } as const satisfies Record.ReadonlyRecord<Configured, string>, (dependency, kind) => ({
        package: dependency,
        child: (_: string, file: string, report: string) => Effect.succeed({ script: fileURLToPath(import.meta.resolve('#/plugins/vite-config')), args: [kind, file, report], env: {} }),
        report: Schema.decodeEffect(Report),
    })),
    playwright: {
        package: '@playwright/test',
        child: Effect.fnUntraced(function* (manifest: string, _: string, report: string) {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const { bin } = yield* Effect.flatMap(fs.readFileString(manifest), Schema.decodeEffect(Bin));
            const script = yield* Effect.fromOption(Record.get(bin, 'playwright'));
            return { script: path.resolve(path.dirname(manifest), script), args: ['test', '--list', '--pass-with-no-tests', '--reporter=json'], env: { PLAYWRIGHT_JSON_OUTPUT_FILE: report } };
        }),
        report: (text: string) => Effect.map(Schema.decodeEffect(Listing)(text), ({ config, suites }) => ({ found: suites.length > 0, written: config.projects.map(({ outputDir }) => outputDir), loaded: Array.empty<string>() })),
    },
};
const RUNNERS = {
    vitest: { run: ['vitest', 'run'], description: 'Runs every Vitest project of the suite with vitest run', example: ['--project', '<name>'] },
    playwright: { run: ['playwright', 'test'], description: 'Runs every Playwright project of the suite with playwright test', example: ['--debug=cli'] },
} as const satisfies Partial<Record<keyof typeof READS, unknown>>;

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
                Array.filterMap(
                    holding
                        .filter(({ parsed }) => parsed.options.noEmit !== true)
                        .flatMap(
                            ({ parsed: { options } }) =>
                                [
                                    [options.outDir, `**/*.{js,cjs,mjs,jsx${options.resolveJsonModule === true ? ',json' : ''},d.ts,d.cts,d.mts}{,.map}`],
                                    [options.declarationDir, '**/*.{d.ts,d.cts,d.mts}{,.map}'],
                                ] as const,
                        ),
                    ([at, emitted]) => (at === undefined ? Result.failVoid : Result.succeed(path.join(relative(at), emitted))),
                ),
            ),
            chain: Array.dedupe(present.flatMap(({ chain }) => chain.map(relative))).filter((at) => !at.startsWith(`${root}/`)),
        };
    });
}, Effect.scoped);

const listed = Effect.fnUntraced(
    function* (workspaceRoot: string, { root, file, kind }: Read) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const folder = path.join(workspaceRoot, root);
        const { package: dependency, child, report: decoded } = READS[kind];
        const manifest = yield* Effect.try(() => createRequire(path.join(folder, 'package.json')).resolve(`${dependency}/package.json`));
        const report = path.join(yield* fs.makeTempDirectoryScoped(), 'report.json');
        const { script, args, env } = yield* child(manifest, path.join(workspaceRoot, file), report);
        const handle = yield* ChildProcess.make(process.execPath, [script, ...args], { cwd: folder, env, extendEnv: true, stdin: 'ignore' });
        const [, stderr, code] = yield* Effect.all([Stream.runDrain(handle.stdout), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
        const { found, written, loaded } = code === 0 ? yield* Effect.flatMap(fs.readFileString(report), decoded) : yield* TypeScriptError.make({ message: `${file}: ${kind} read exited ${code}: ${stderr.trim()}` });
        const relative = Array.dedupe(written.map((at) => path.relative(workspaceRoot, at)));
        return { root, kind, found, written: relative.filter((at) => !relative.some((other) => at.startsWith(`${other}/`))), loaded };
    },
    Effect.scoped,
    (effect, _, { file, kind }) => Effect.mapError(effect, (error) => (Schema.is(TypeScriptError)(error) ? error : TypeScriptError.make({ message: `${file}: ${kind} read: ${error.message}`, cause: error }))),
);

const resolved = Effect.fnUntraced(function* (workspaceRoot: string, reads: readonly Read[]) {
    const path = yield* Path.Path;
    const results = yield* Effect.forEach(reads, (read) => Effect.result(listed(workspaceRoot, read)), { concurrency: 'unbounded' });
    const succeeded = Array.getSuccesses(results);
    const loaded = yield* Effect.promise(() => globWithWorkspaceContext(workspaceRoot, Array.dedupe(Array.getSomes(succeeded.flatMap(({ loaded: urls }) => urls).map((url) => Option.liftThrowable(fileURLToPath)(url))).map((at) => path.relative(workspaceRoot, at)))));
    return {
        bundles: Record.fromIterableWith(
            succeeded.filter(({ kind }) => kind === 'vite'),
            ({ root, written }) => [root, written],
        ),
        tests: Array.filterMap(succeeded, ({ root, kind, found, written }) => (kind === 'vite' || !found ? Result.failVoid : Result.succeed({ root, runner: kind, outputs: written }))),
        failures: Array.filterMap(Array.zip(reads, results), ([{ file }, result]) => Result.map(Result.flip(result), (failure): [string, Error] => [file, new Error(failure.message)])),
        loaded,
        digest: yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...loaded])),
    };
});

const targets = (
    path: Path.Path,
    plugin: { readonly name: string; readonly inputs: NonNullable<TargetConfiguration['inputs']> },
    program: Program,
    held: ReadonlySet<Kind>,
    bundles: Option.Option<readonly string[]>,
    tests: readonly { readonly runner: Runner; readonly outputs: readonly string[] }[],
    external: readonly string[],
    outputPath: Option.Option<string>,
): Record.ReadonlyRecord<string, TargetConfiguration> => {
    const metadata = (description: string, help: readonly string[], example: readonly string[]) => ({ technologies: TECHNOLOGIES, description, help: { command: line([...help, '--help']), example: { args: [...example] } } });
    const command = (argv: readonly string[]) => ({ executor: 'nx:run-commands', options: { command: line(argv), cwd: program.root } });
    const chained = program.chain.map((at) => path.join('{workspaceRoot}', at));
    const compiled = (held.has('cloudflare') ? [{ dependentTasksOutputFiles: '**/*.d.ts' }] : []) satisfies NonNullable<TargetConfiguration['inputs']>;
    const tsc: TargetConfiguration = {
        executor: 'nx:run-commands',
        options: { commands: program.commands.map(line), cwd: program.root },
        cache: true,
        inputs: ['default', '^default', ...chained, ...compiled, { externalDependencies: ['@typescript/native', ...external] }],
        ...(held.has('cloudflare') ? { dependsOn: ['build--cf'] } : {}),
        outputs: [],
    };
    const storybook = Option.map(outputPath, (at) => path.join(at, 'storybook', '{projectName}'));
    const inferred: Record.ReadonlyRecord<string, TargetConfiguration> = {
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
        ...Record.fromIterableWith(Option.toArray(bundles), (folders): [string, TargetConfiguration] => [
            'build--vite',
            {
                ...command(['vite', 'build']),
                cache: true,
                inputs: ['default', '^default', ...chained, { externalDependencies: ['vite'] }],
                outputs: folders.map((at) => path.join('{workspaceRoot}', at)),
                metadata: metadata('Builds every environment of the Vite config with vite build', ['vite', 'build'], ['--mode', 'production']),
            },
        ]),
        ...Record.fromIterableWith(tests, ({ runner, outputs }): [string, TargetConfiguration] => [
            `test--${runner}`,
            {
                ...command(RUNNERS[runner].run),
                cache: true,
                inputs: ['default', '^default', ...chained, { env: 'CI' }, { externalDependencies: [] }],
                outputs: outputs.map((at) => path.join('{workspaceRoot}', at)),
                metadata: metadata(RUNNERS[runner].description, RUNNERS[runner].run, RUNNERS[runner].example),
            },
        ]),
        ...(held.has('publint') && program.outputs.length > 0
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
                    Effect.promise(() => globWithWorkspaceContext(workspaceRoot, [`**/${pattern}`])).pipe(Effect.flatMap(Effect.filter(accepted, { concurrency: 'unbounded' })), Effect.map(Array.map((file) => ({ root: path.join(file, ...Array.replicate('..', pattern.split('/').length)), kind, file })))),
                { concurrency: 'unbounded' },
            ).pipe(Effect.map(Array.flatten));
            const resolves = (specifier: string, root: string) => Option.liftThrowable(findPackageJSON)(specifier, pathToFileURL(path.join(workspaceRoot, root, 'package.json')));
            const suites = Array.getSomes(
                Array.cartesian(projects, Record.keys(RUNNERS)).map(([root, kind]): Option.Option<Read> => {
                    const configs = Array.filterMap(markers, (marker) => (marker.root === root && marker.kind === kind ? Result.succeed(marker.file) : Result.failVoid));
                    const preferred = Option.orElse(
                        Array.findFirst(configs, (file) => path.basename(file).startsWith('vitest.')),
                        () => Array.head(configs),
                    );
                    return Option.zipLeft(
                        Option.map(preferred, (file) => ({ root, kind, file })),
                        resolves(READS[kind].package, root),
                    );
                }),
            );
            const reads: readonly Read[] = [...markers.filter(({ root, kind }) => kind === 'vite' && projects.includes(root)).map(({ root, file }) => ({ root, file, kind: 'vite' as const })), ...suites];
            const installed = (specifier: string) => Option.isSome(resolves(specifier, '.'));
            const linted = installed('publint') ? ['publint' as const] : [];
            const held = (root: string) => new Set<Kind>([...Array.filterMap(markers, (marker) => (marker.root === root ? Result.succeed(marker.kind) : Result.failVoid)), ...linted]);
            const external = installed('@effect/tsgo') ? ['@effect/tsgo'] : [];
            const modules = (yield* fs.readDirectory(import.meta.dirname)).toSorted().map((module) => path.join(import.meta.dirname, module));
            const hash = hashArray([
                yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, ['**/tsconfig*.json', getLockFileName(detectPackageManager(workspaceRoot)), ...reads.map(({ file }) => file)])),
                ...(yield* Effect.promise(() =>
                    globWithWorkspaceContext(
                        workspaceRoot,
                        suites.map(({ root }) => path.join(root, '**')),
                    ),
                )),
                ...(yield* Effect.forEach(modules, (module) => fs.readFileString(module))),
                hashObject(decoded),
                ...projects,
            ]);
            const located = yield* Effect.fromNullishOr(findPackageJSON(import.meta.url));
            const { name } = yield* Effect.flatMap(fs.readFileString(located), Schema.decodeEffect(Manifest));
            const tracked = yield* Effect.promise(() => globWithWorkspaceContext(workspaceRoot, [path.relative(workspaceRoot, located)]));
            const plugin = { name, inputs: tracked.length === 0 ? [{ externalDependencies: [name] }] : [{ input: 'default', projects: [name] }, { externalDependencies: [] }] };
            const digest = (sources: readonly string[]) => Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...sources]));
            const cache = new PluginCache<{
                readonly programs: readonly Program[];
                readonly bundles: Readonly<Record<string, readonly string[]>>;
                readonly tests: readonly { readonly root: string; readonly runner: Runner; readonly outputs: readonly string[] }[];
                readonly loaded: readonly string[];
                readonly digest: string;
            }>(path.join(workspaceDataDirectory, `${path.basename(name)}-${hashObject(decoded)}.hash`));
            const valid = yield* Effect.filter(Option.toArray(Option.fromNullishOr(cache.get(hash))), (entry) => Effect.map(digest(entry.loaded), (current) => current === entry.digest));
            const { read, failures } = yield* Option.match(Array.head(valid), {
                onSome: (entry) => Effect.succeed({ read: entry, failures: Array.empty<[string, Error]>() }),
                onNone: () =>
                    Effect.all([programs(workspaceRoot, projects), resolved(workspaceRoot, reads)], { concurrency: 'unbounded' }).pipe(
                        Effect.map(([found, { failures: failed, ...entry }]) => ({ read: { programs: found, ...entry }, failures: failed })),
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
                {
                    projects: {
                        [program.root]: {
                            metadata: { technologies: TECHNOLOGIES },
                            targets: targets(
                                path,
                                plugin,
                                program,
                                held(program.root),
                                Record.get(read.bundles, program.root),
                                read.tests.filter(({ root }) => root === program.root),
                                external,
                                Option.fromUndefinedOr(decoded.outputPath),
                            ),
                            ...released(program.root),
                        },
                    },
                },
            ]);
            return failures.length === 0 ? nodes : yield* Effect.fail(new AggregateCreateNodesError(failures, nodes));
        }).pipe(
            Effect.mapError((error) => (error instanceof AggregateCreateNodesError ? error : new AggregateCreateNodesError([[null, new Error(error.message)]], []))),
            Effect.provide(layer),
            Effect.runPromise,
        ),
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
