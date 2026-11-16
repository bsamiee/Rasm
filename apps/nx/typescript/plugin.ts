import { createRequire, findPackageJSON } from 'node:module';
import { basename, dirname, extname, join, relative, resolve } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import type { loadConfig } from '@cloudflare/config';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesContext, createNodesFromFiles, detectPackageManager, getPackageManagerCommand, hashArray, normalizePath, type PackageManager, type ProjectConfiguration, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { getFilesInDirectoryUsingContext, getLockFileName, getNamedInputs, hashFile, hashObject, hashWithWorkspaceContext, mergeTargetConfigurations, type PackageJson, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import type { API, ParsedCommandLine } from '@typescript/native/sync';
import { Array, Cause, Effect, Equal, FileSystem, Match, Option, Predicate, Record, Result, Schema, UndefinedOr } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Program = Effect.Success<ReturnType<typeof resolved>>;
type Verbs = Readonly<Record<string, Readonly<Record<string, TargetConfiguration>>>>;
type Stated = Readonly<Record<string, Pick<TargetConfiguration, 'inputs'>>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const CONFIG = 'tsconfig.json';
const MANIFEST = 'package.json';
const WORKER = 'cloudflare.config.ts';
const UNPACKAGED: NonNullable<TargetConfiguration['inputs']> = ['default', '^default', { externalDependencies: [] }];
const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#package'))).name;
const MANAGERS = {
    npm: { argv: ['npm', 'pack'], to: (at: string) => ['--pack-destination', at], workspace: { json: join('{workspaceRoot}', MANIFEST), fields: ['workspaces'] }, upgrade: ['npm', 'update', '--save', '--workspaces', '--include-workspace-root'], example: ['--dry-run'] },
    pnpm: { argv: ['pnpm', 'pack'], to: (at: string) => ['--out', join(at, '%s.tgz')], workspace: join('{workspaceRoot}', 'pnpm-workspace.yaml'), upgrade: ['pnpm', 'update', '--recursive', '--latest'], example: ['--interactive'] },
    yarn: { argv: ['yarn', 'pack'], to: (at: string) => ['--out', join(at, '%s.tgz')], workspace: join('{workspaceRoot}', '.yarnrc.yml'), upgrade: ['yarn', 'up', '*'], example: ['--interactive'] },
    bun: { argv: ['bun', 'pm', 'pack'], to: (at: string) => ['--destination', at], workspace: { json: join('{workspaceRoot}', MANIFEST), fields: ['workspaces'] }, upgrade: ['bun', 'update', '--recursive', '--latest'], example: ['--dry-run'] },
} as const satisfies Record<PackageManager, { readonly argv: readonly string[]; readonly to: (at: string) => readonly string[]; readonly workspace: NonNullable<TargetConfiguration['inputs']>[number]; readonly upgrade: readonly string[]; readonly example: readonly string[] }>;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({
    outputPath: Schema.optionalKey(Schema.String),
    compilerPackages: Schema.NonEmptyArray(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed(['typescript'] as const))),
}).annotate({
    description:
        'outputPath: workspace folder the package manager packs each tarball to, {projectName} naming the project, absent packs into the package folder. compilerPackages: npm packages every tsc target hashes as external dependencies, the first the native TypeScript package whose tsc runs and whose ./sync API reads each tsconfig.json',
});
const Input = Schema.declare((input: unknown): input is NonNullable<TargetConfiguration['inputs']>[number] => Predicate.isString(input) || Predicate.isObject(input));
const Manifest = Schema.fromJsonString(
    Schema.Struct({
        version: Schema.optionalKey(Schema.String),
        os: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
        nx: Schema.optionalKey(Schema.Struct({ targets: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ inputs: Schema.optionalKey(Schema.mutable(Schema.Array(Input))) }))) })),
    }),
).annotate({ description: "The version, os, and nx.targets rows of a project's package.json, whose inputs Nx merges over the inferred targets after every plugin" });
const Exported = Schema.fromJsonString(Schema.Struct({ exports: Schema.Struct({ '.': Schema.Struct({ import: Schema.String }) }) })).annotate({ description: 'The import entry of the root export of @cloudflare/config, the package cf loads cloudflare.config.ts through' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const run = <A, E>(effect: Effect.Effect<A, E, FileSystem.FileSystem>) => Effect.runPromise(Effect.provide(effect, layer));

const rendered = (cause: Cause.Cause<unknown>) => Effect.fail(new Cause.UnknownError(cause, Cause.pretty(cause)));

const own = Effect.runSync(
    Effect.cached(
        Effect.gen(function* () {
            const names = yield* FileSystem.FileSystem.use((fs) => fs.readDirectory(import.meta.dirname));
            return hashArray(names.toSorted().flatMap((name) => [name, ...Array.fromNullishOr(hashFile(join(import.meta.dirname, name)))]));
        }),
    ),
);

const manifested = Effect.fnUntraced(function* (workspaceRoot: string, root: string) {
    const fs = yield* FileSystem.FileSystem;
    return [root, yield* Effect.result(Effect.flatMap(fs.readFileString(join(workspaceRoot, root, MANIFEST)), Schema.decodeEffect(Manifest)))] as const;
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
                                .map((input) => (Predicate.isObject(input) && 'dependentTasksOutputFiles' in input ? { ...input, transitive: true } : input)),
                        ),
                    ],
                },
            ],
            ...Record.collect(tools, (tool, target): readonly [string, TargetConfiguration] => [`${verb}--${tool}`, { ...target, metadata: { technologies: [technology], ...target.metadata } }]),
        ]).flat(),
    );

const projected = (root: string, technology: string, verbs: Verbs, stated: Stated, extra: Partial<ProjectConfiguration> = {}) => ({ root, metadata: { technologies: [technology] }, ...extra, targets: aggregated(verbs, stated, technology) }) satisfies ProjectConfiguration;

const installed = (name: string, from: string) => Option.isSome(Option.flatMap(Option.liftThrowable(findPackageJSON)(name, pathToFileURL(from)), Option.fromUndefinedOr));

const stamped = (workspaceRoot: string, sources: readonly string[]) => Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, Array.dedupe(sources.flatMap((at) => [at, join(dirname(at), '*')]))));

const kept = Effect.fnUntraced(function* (cache: PluginCache<Program>, workspaceRoot: string, lock: string, config: string) {
    const root = dirname(config);
    const [configured, listed] = yield* Effect.all([Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [`${root}/*.json`, `${root}/**/tsconfig*.json`, lock])), Effect.promise(() => getFilesInDirectoryUsingContext(workspaceRoot, root))], { concurrency: 'unbounded' });
    const key = hashArray([configured, ...listed, config]);
    const program = yield* Effect.findFirst(Array.fromNullishOr(cache.get(key)), ({ sources, stamp }) => Effect.map(stamped(workspaceRoot, sources), Equal.equals(stamp)));
    return { config, key, program };
});

const referenced = ({ projectReferences = [] }: ParsedCommandLine) => projectReferences.map(({ path: at }) => (extname(at) === '.json' ? at : join(at, CONFIG)));

const solved = Effect.fnUntraced(function* (workspaceRoot: string, compiler: string, configs: Array.NonEmptyReadonlyArray<string>) {
    const unopened = (cause: unknown) => new Cause.UnknownError(cause, `${compiler}/sync opens no snapshot of ${workspaceRoot}`);
    const native = yield* Effect.tryPromise({
        try: (): Promise<{ readonly API: typeof API }> => import(pathToFileURL(createRequire(resolve(workspaceRoot, Array.headNonEmpty(configs))).resolve(`${compiler}/sync`)).href),
        catch: (cause) => new Cause.UnknownError(cause, `${workspaceRoot} resolves no ${compiler}/sync`),
    });
    const api = yield* Effect.acquireDisposable(Effect.sync(() => new native.API({ cwd: workspaceRoot })));
    const parsed = (config: string) => {
        const file = resolve(workspaceRoot, config);
        const read = api.parseConfigFile(file);
        return { errors: read.errors.map(({ text }) => text), owned: [file, ...referenced(read).filter((at) => dirname(at) === dirname(file))] };
    };
    const reads = configs.map((config) => [config, Result.try({ try: () => parsed(config), catch: (cause) => new Cause.UnknownError(cause, `${config} does not parse as a tsconfig`) })] as const);
    const files = Array.dedupe(Array.getSuccesses(reads.map(([, read]) => read)).flatMap(({ owned }) => owned));
    const snapshot = yield* Effect.acquireDisposable(Effect.try({ try: () => api.createSnapshot({ openProjects: files }), catch: unopened }));
    const configured = (file: string) => Option.map(Option.fromNullishOr(snapshot.getConfiguredProject(file)), (project) => [file, { file, parsed: project.parsedCommandLine, chain: project.program.getConfigFileNames() }] as const);
    const opened = new Map(yield* Effect.try({ try: () => Array.getSomes(files.map(configured)), catch: unopened }));
    const members = (owned: readonly string[]) => Array.getSomes(owned.map((file) => Option.fromUndefinedOr(opened.get(file))));
    return reads.map(([config, read]) => [config, Result.map(read, ({ errors, owned }) => ({ config, errors, members: members(owned) }))] as const);
}, Effect.scoped);

const resolved = Effect.fnUntraced(function* (workspaceRoot: string, solution: Effect.Success<ReturnType<typeof solved>>[number][1]) {
    const { config, errors, members } = yield* Effect.fromResult(solution);
    const folder = join(workspaceRoot, dirname(config));
    const local = (at: string) => normalizePath(relative(workspaceRoot, at));
    const holding = members.filter(({ parsed }) => parsed.fileNames.length > 0);
    const checked = Option.match(
        Array.findFirst(holding, ({ file }) => local(file) === config),
        { onSome: Array.of, onNone: () => holding },
    );
    const built = checked.some(({ parsed }) => (parsed.projectReferences ?? []).length > 0);
    const outputs = holding
        .filter(({ parsed }) => parsed.options.noEmit !== true)
        .flatMap(({ file, parsed: { options } }) => {
            const emitted = options.resolveJsonModule === true ? '**/*.{js,cjs,mjs,jsx,json,d.ts,d.cts,d.mts}{,.map}' : '**/*.{js,cjs,mjs,jsx,d.ts,d.cts,d.mts}{,.map}';
            const at = resolve(options.outDir ?? dirname(file), options.outDir === undefined || options.rootDir === undefined ? '.' : relative(options.rootDir, dirname(file)));
            return [
                join(options.outDir ?? dirname(file), emitted),
                ...(options.declarationDir === undefined ? [] : [join(options.declarationDir, '**/*.{d.ts,d.cts,d.mts}{,.map}')]),
                ...(built || options.incremental === true || options.composite === true ? [options.tsBuildInfoFile ?? join(at, `${basename(file, '.json')}.tsbuildinfo`)] : []),
            ].map(local);
        });
    const sources = Array.dedupe(holding.flatMap(({ chain, parsed }) => [...chain, ...parsed.fileNames]).map(local)).filter((at) => !(at.startsWith('..') || at.startsWith(`${dirname(config)}/`) || at.split('/').includes('node_modules')));
    yield* Array.match(errors, { onEmpty: () => Effect.void, onNonEmpty: (reasons) => Effect.fail(new Cause.UnknownError(reasons, `${config} does not compile its options: ${reasons.join('; ')}`)) });
    return {
        commands: checked.map(({ file, parsed }) => ['tsc', (parsed.projectReferences ?? []).length > 0 ? '--build' : '--project', relative(folder, file)].map(quoteShellArg).join(' ')),
        configs: checked.map(({ file }) => relative(folder, file)),
        outputs: Array.dedupe(outputs),
        sources,
        stamp: yield* stamped(workspaceRoot, sources),
    };
});

const compiled = (root: string, { commands, configs, outputs, sources }: Program, packages: readonly string[], upstream: string, linted: boolean, synced: boolean, exec: string): Verbs => {
    const metadata = (description: string, example: readonly string[]) => ({ description, help: { command: `${exec} tsc --help`, example: { args: [...example] } } });
    const tsc = {
        executor: 'nx:run-commands',
        options: { commands, cwd: root },
        dependsOn: ['codegen--*', '^codegen', '^build--tsc'],
        cache: true,
        inputs: ['default', upstream, ...sources.map((at) => join('{workspaceRoot}', at)), { dependentTasksOutputFiles: '**/*', transitive: true }, { externalDependencies: [...packages] }],
        outputs: [],
        ...(synced && { syncGenerators: ['@nx/js:typescript-sync'] }),
    } satisfies TargetConfiguration;
    return Array.match(outputs, {
        onEmpty: () => ({ check: { tsc: { ...tsc, metadata: metadata('Type-checks every program of the project with tsc', ['--extendedDiagnostics']) } } }),
        onNonEmpty: (written) => {
            const build = { tsc: { ...tsc, outputs: written.map((at) => join('{workspaceRoot}', at)), metadata: metadata('Emits JavaScript and declarations with tsc, type-checking every program it builds', ['--listEmittedFiles']) } };
            const removed = Record.collect(build, (tool) => `build--${tool}`);
            return {
                build,
                check: {
                    tsc: { executor: 'nx:noop', dependsOn: ['build--tsc'], inputs: tsc.inputs, metadata: metadata('Type-checks through the build--tsc compile with tsc', ['--extendedDiagnostics']) },
                    ...(linted && {
                        publint: {
                            command: 'publint --strict',
                            options: { cwd: root },
                            dependsOn: ['build--*'],
                            cache: true,
                            inputs: ['default', { dependentTasksOutputFiles: '**/*' }, { externalDependencies: ['publint'] }],
                            outputs: [],
                            metadata: { description: 'Lints the tarball the package manager packs against its package.json with publint, warnings failing through --strict', help: { command: `${exec} publint run --help`, example: { args: ['--level', 'warning'] } } },
                        },
                    }),
                },
                clean: {
                    tsc: {
                        executor: `${distribution.name}:clean`,
                        options: { targets: removed, commands: configs.map((config) => ['tsc', '--build', '--clean', config]), cwd: root },
                        inputs: ['default', '^default', { externalDependencies: [...packages] }],
                        metadata: { description: `Removes the outputs ${removed.join(', ')} declare and each program output with tsc --build --clean`, help: { command: `${exec} tsc --build --help`, example: { args: ['--dry'] } } },
                    },
                },
            };
        },
    });
};

const tarball = (manager: PackageManager, exec: string, { outputPath }: typeof Options.Type, { version, os, nx }: typeof Manifest.Type, root: string) => {
    const { argv, to, workspace } = MANAGERS[manager];
    const folder = Option.fromUndefinedOr(outputPath);
    const pack = {
        [manager]: {
            command: [...argv, ...Option.match(folder, { onNone: () => [], onSome: (at) => to(relative(root, at)) })].map(quoteShellArg).join(' '),
            options: { cwd: root },
            dependsOn: ['build--*'],
            cache: true,
            inputs: ['default', workspace, { dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }],
            outputs: [
                join(
                    '{workspaceRoot}',
                    Option.getOrElse(folder, () => root),
                    '*.tgz',
                ),
            ],
            metadata: { description: `Packs the package into one tarball named for it with ${argv.join(' ')}`, help: { command: `${argv.join(' ')} --help`, example: { args: ['--json'] } } },
        },
    } satisfies Record<string, TargetConfiguration>;
    const removed = Record.collect(pack, (tool) => `pack--${tool}`);
    const verbs: Verbs = {
        pack,
        clean: { [manager]: { executor: `${distribution.name}:clean`, options: { targets: removed }, inputs: UNPACKAGED, metadata: { description: `Removes the outputs ${removed.join(', ')} declare`, help: { command: `${exec} nx run --help`, example: { options: { targets: removed } } } } } },
        attach: {
            gh: {
                executor: `${distribution.name}:attach`,
                dependsOn: removed,
                inputs: [{ dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }],
                metadata: { description: `Uploads the files ${removed.join(', ')} wrote to the GitHub release of the tag nx release made for the project at the checked-out commit with gh release upload`, help: { command: 'gh release upload --help', example: { options: { dryRun: true } } } },
            },
        },
    };
    const systems = os.filter((system) => !system.startsWith('!')).map((system) => `os:${system}`);
    return UndefinedOr.match(version, {
        onUndefined: () => Option.map(Option.liftPredicate(systems, Array.isReadonlyArrayNonEmpty), (tags): ProjectConfiguration => ({ root, tags: [...tags] })),
        onDefined: () => Option.some(projected(root, 'npm', verbs, nx?.targets ?? {}, { tags: ['release', ...systems], release: {} })),
    });
};

const upgraded = (manager: PackageManager, { nx }: typeof Manifest.Type) => {
    const { upgrade, example } = MANAGERS[manager];
    const command = upgrade.map(quoteShellArg).join(' ');
    return Option.some(
        projected(
            '.',
            manager,
            {
                upgrade: {
                    [manager]: { command, inputs: [{ externalDependencies: [] }], metadata: { description: `Moves the dependency requirements of every workspace package and the lockfile to newer releases with ${command}`, help: { command: `${upgrade.slice(0, 2).join(' ')} --help`, example: { args: [...example] } } } },
                },
            },
            nx?.targets ?? {},
        ),
    );
};

const worker = Effect.fnUntraced(function* (context: CreateNodesContext, exec: string, stated: Stated, file: string) {
    const fs = yield* FileSystem.FileSystem;
    const [root, at] = [dirname(file), join(context.workspaceRoot, file)];
    const unresolved = (cause: unknown) => new Cause.UnknownError(cause, `${file} resolves no @cloudflare/config through cf`);
    const configured = yield* Effect.flatMap(Effect.try({ try: () => findPackageJSON('@cloudflare/config', pathToFileURL(createRequire(at).resolve('cf/package.json'))), catch: unresolved }), Effect.fromNullishOr);
    const { exports } = yield* Effect.flatMap(fs.readFileString(configured), Schema.decodeEffect(Exported));
    const cloudflare = yield* Effect.tryPromise({ try: (): Promise<{ readonly loadConfig: typeof loadConfig }> => import(pathToFileURL(join(dirname(configured), exports['.'].import)).href), catch: unresolved });
    const { dependencies } = yield* Effect.tryPromise({ try: () => cloudflare.loadConfig(at), catch: (cause) => new Cause.UnknownError(cause, `${file} does not load as a Cloudflare config`) });
    const sources = Array.map(Array.fromIterable(dependencies), (dependency) => normalizePath(relative(context.workspaceRoot, dependency))).filter((dependency) => !(dependency.startsWith('..') || dependency.startsWith(`${root}/`) || dependency.split('/').includes('node_modules')));
    const typed = installed('@cloudflare/workers-types', at);
    const codegen = {
        cf: {
            command: typed ? 'cf workers types --include-runtime=false' : 'cf workers types',
            options: { cwd: root },
            dependsOn: ['^codegen'],
            cache: true,
            inputs: ['default', 'production' in getNamedInputs(root, context) ? '^production' : '^default', ...sources.map((dependency) => join('{workspaceRoot}', dependency)), { externalDependencies: ['cf'] }],
            outputs: ['{projectRoot}/.cloudflare/types'],
            metadata: {
                description: typed ? `Generates the Worker binding types from ${WORKER} with cf workers types, runtime types coming from @cloudflare/workers-types` : `Generates the Worker binding and runtime types from ${WORKER} with cf workers types`,
                help: { command: `${exec} cf workers types --help`, example: { args: ['--mode', 'production'] } },
            },
        },
    } satisfies Record<string, TargetConfiguration>;
    const removed = Record.collect(codegen, (tool) => `codegen--${tool}`);
    return projected(
        root,
        'cloudflare',
        {
            codegen,
            clean: { cf: { executor: `${distribution.name}:clean`, options: { targets: removed }, inputs: UNPACKAGED, metadata: { description: `Removes the outputs ${removed.join(', ')} declare`, help: { command: `${exec} nx run --help`, example: { options: { targets: removed } } } } } },
        },
        stated,
    );
});

const looked = <A, E>(table: ReadonlyMap<string, Result.Result<A, E>>, at: string) => Effect.flatMap(Effect.fromNullishOr(table.get(at)), Effect.fromResult);

const nodes = Effect.fnUntraced(function* (files: readonly string[], options: typeof Options.Encoded | undefined, context: CreateNodesContext) {
    const { workspaceRoot } = context;
    const [settings, hashed] = yield* Effect.all([Schema.decodeEffect(Options)(options ?? {}), own]);
    const held = new Set(files.filter((file) => basename(file) === MANIFEST && dirname(file) !== '.').map(dirname));
    const nested = files.filter((file) => file === MANIFEST || held.has(dirname(file)));
    const configs = nested.filter((file) => basename(file) === CONFIG);
    const manager = detectPackageManager(workspaceRoot);
    const { exec } = getPackageManagerCommand(manager);
    const synced = installed('@nx/js', join(workspaceRoot, MANIFEST));
    const cache = new PluginCache<Program>(join(workspaceDataDirectory, `typescript-${hashArray([hashed, hashObject(options ?? {})])}.hash`));
    const manifests = new Map(yield* Effect.forEach(Array.dedupe(nested.map(dirname)), (root) => manifested(workspaceRoot, root), { concurrency: 'unbounded' }));
    const rows = yield* Effect.forEach(configs, (config) => kept(cache, workspaceRoot, getLockFileName(manager), config), { concurrency: 'unbounded' });
    const unread = rows.filter(({ program }) => Option.isNone(program));
    const solutions = yield* Array.match(unread, {
        onEmpty: () => Effect.succeed([]),
        onNonEmpty: (missed) =>
            solved(
                workspaceRoot,
                Array.headNonEmpty(settings.compilerPackages),
                Array.map(missed, ({ config }) => config),
            ),
    });
    const fresh = yield* Effect.forEach(solutions, ([, solution]) => Effect.result(resolved(workspaceRoot, solution)), { concurrency: 'unbounded' });
    const stored = Array.getSuccesses(Array.zipWith(unread, fresh, ({ key }, program) => Result.map(program, (value) => [key, value] as const)));
    yield* Effect.forEach(stored, ([key, value]) => Effect.sync(() => cache.set(key, value)));
    yield* Array.isReadonlyArrayNonEmpty(stored) ? Effect.sync(() => cache.writeToDisk()) : Effect.void;
    const programs = new Map<string, Result.Result<Program, Cause.UnknownError>>([...Array.getSomes(rows.map(({ config, program }) => Option.map(program, (cached) => [config, Result.succeed(cached)] as const))), ...Array.zipWith(unread, fresh, ({ config }, program) => [config, program] as const)]);
    const read = Effect.fnUntraced(function* (file: string) {
        const root = dirname(file);
        const manifest = yield* looked(manifests, root);
        const stated = manifest.nx?.targets ?? {};
        const typed = (program: Program) => Option.some(projected(root, 'typescript', compiled(root, program, settings.compilerPackages, 'production' in getNamedInputs(root, context) ? '^production' : '^default', manifest.version !== undefined && installed('publint', join(workspaceRoot, file)), synced, exec), stated));
        const loadable = Effect.sync(() => installed('cf', join(workspaceRoot, file)));
        return yield* Match.value(file).pipe(
            Match.when(MANIFEST, () => Effect.succeed(upgraded(manager, manifest))),
            Match.when(
                (at) => basename(at) === CONFIG,
                () => Effect.map(looked(programs, file), typed),
            ),
            Match.when(
                (at) => basename(at) === WORKER,
                () => Effect.when(worker(context, exec, stated, file), loadable),
            ),
            Match.orElse(() => Effect.succeed(tarball(manager, exec, settings, manifest, root))),
        );
    });
    const each = (file: string) =>
        run(
            Effect.catchCause(
                Effect.map(read(file), (project) => ({ projects: Record.getSomes({ [dirname(file)]: project }) })),
                rendered,
            ),
        );
    return yield* Effect.tryPromise({
        try: () => createNodesFromFiles(each, nested, options, context),
        catch: (cause) => (cause instanceof AggregateCreateNodesError ? cause : new AggregateCreateNodesError([[null, new Cause.UnknownError(cause, `${PLUGIN} infers no project`)]], [])),
    });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = [`**/{${CONFIG},${MANIFEST},${WORKER}}`, (files, options, context) => run(nodes(files, options, context))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
