import { createRequire, findPackageJSON } from 'node:module';
import { basename, dirname, extname, join, relative, resolve } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesContext, createNodesFromFiles, detectPackageManager, getPackageManagerCommand, normalizePath, type PackageManager, type ProjectConfiguration, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { calculateHashesForCreateNodes, getLockFileName, getNamedInputs, hashObject, hashWithWorkspaceContext, mergeTargetConfigurations, type PackageJson, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import type { ParsedCommandLine } from '@typescript/native/sync';
import { Array, Cause, Effect, Equal, FileSystem, Match, Option, type PlatformError, Predicate, Record, Result, Schema, UndefinedOr } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Solution = Result.Result.Success<Effect.Success<ReturnType<typeof solved>>[number][1]>;
type Program = Effect.Success<ReturnType<typeof resolved>>;
type Verbs = Readonly<Record<string, Readonly<Record<string, TargetConfiguration>>>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const CONFIG = 'tsconfig.json';
const MANIFEST = 'package.json';
const WORKER = 'cloudflare.config.ts';
const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#package'))).name;
const PACKS = {
    npm: { argv: ['npm', 'pack'], to: (at: string) => ['--pack-destination', at], workspace: MANIFEST },
    pnpm: { argv: ['pnpm', 'pack'], to: (at: string) => ['--out', join(at, '%s.tgz')], workspace: 'pnpm-workspace.yaml' },
    yarn: { argv: ['yarn', 'pack'], to: (at: string) => ['--out', join(at, '%s.tgz')], workspace: '.yarnrc.yml' },
    bun: { argv: ['bun', 'pm', 'pack'], to: (at: string) => ['--destination', at], workspace: MANIFEST },
} as const satisfies Record<PackageManager, { readonly argv: readonly string[]; readonly to: (at: string) => readonly string[]; readonly workspace: string }>;

// --- [MODELS] --------------------------------------------------------------------------

const Manifest = Schema.fromJsonString(Schema.Struct({ version: Schema.optionalKey(Schema.String), os: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) }));
const Options = Schema.Struct({
    outputPath: Schema.optionalKey(Schema.String),
    releaseTag: Schema.optionalKey(Schema.String),
    platformTagPrefix: Schema.optionalKey(Schema.String),
    compilerPackages: Schema.NonEmptyArray(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed(['typescript'] as const))),
}).annotate({
    description:
        'outputPath: workspace folder the package manager packs each tarball to, {projectName} naming the project, absent packs into the package folder. releaseTag: tag of every package stating a version, absent tags none. platformTagPrefix: prefix of a tag per platform the package.json os field allows, absent tags none. compilerPackages: npm packages every tsc target hashes as external dependencies, the first the native TypeScript package whose tsc runs and whose ./sync API reads each tsconfig.json',
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

const projected = Effect.fnUntraced(function* (workspaceRoot: string, root: string, technology: string, verbs: Verbs, extra: Partial<ProjectConfiguration> = {}) {
    const stated = yield* manifested(workspaceRoot, root);
    return { root, metadata: { technologies: [technology] }, ...extra, targets: aggregated(verbs, stated, technology) } satisfies ProjectConfiguration;
});

const installed = (name: string, from: string) => Option.isSome(Option.flatMap(Option.liftThrowable(findPackageJSON)(name, pathToFileURL(from)), Option.fromUndefinedOr));

const stamped = (workspaceRoot: string, sources: readonly string[]) => Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, Array.dedupe(sources.flatMap((at) => [at, join(dirname(at), '*')]))));

const solved = Effect.fnUntraced(function* (workspaceRoot: string, compiler: string, configs: Array.NonEmptyReadonlyArray<string>) {
    const unopened = (cause: unknown) => new Cause.UnknownError(cause, `${compiler}/sync opens no snapshot of ${workspaceRoot}`);
    const { API }: typeof import('@typescript/native/sync') = yield* Effect.tryPromise({
        try: () => import(pathToFileURL(createRequire(resolve(workspaceRoot, Array.headNonEmpty(configs))).resolve(`${compiler}/sync`)).href),
        catch: (cause) => new Cause.UnknownError(cause, `${workspaceRoot} resolves no ${compiler}/sync`),
    });
    const api = yield* Effect.acquireDisposable(Effect.sync(() => new API({ cwd: workspaceRoot })));
    const referenced = ({ projectReferences = [] }: ParsedCommandLine) => projectReferences.map(({ path: at }) => (extname(at) === '.json' ? at : join(at, CONFIG)));
    const parsed = (config: string) => {
        const file = resolve(workspaceRoot, config);
        const read = api.parseConfigFile(file);
        return { errors: read.errors.map(({ text }) => text), owned: [file, ...referenced(read)] };
    };
    const reads = configs.map((config) => [config, Result.try({ try: () => parsed(config), catch: (cause) => new Cause.UnknownError(cause, `${config} does not parse as a tsconfig`) })] as const);
    const files = Array.dedupe(Array.getSuccesses(reads.map(([, read]) => read)).flatMap(({ owned }) => owned));
    const snapshot = yield* Effect.acquireDisposable(Effect.try({ try: () => api.createSnapshot({ openProjects: files }), catch: unopened }));
    const opened = new Map(
        yield* Effect.try({
            try: () =>
                Array.filterMap(files, (file) =>
                    Result.map(
                        Result.fromNullishOr(snapshot.getConfiguredProject(file), () => file),
                        (project) => [file, { file, parsed: project.parsedCommandLine, chain: project.program.getConfigFileNames() }] as const,
                    ),
                ),
            catch: unopened,
        }),
    );
    const members = (owned: readonly string[]) => Array.getSomes(owned.map((file) => Option.fromUndefinedOr(opened.get(file))));
    return reads.map(([config, read]) => [config, Result.map(read, ({ errors, owned }) => ({ config, errors, members: members(owned) }))] as const);
}, Effect.scoped);

const resolved = Effect.fnUntraced(function* (workspaceRoot: string, { config, errors, members }: Solution) {
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

const compiled = (root: string, { commands, configs, outputs, sources }: Program, packages: readonly string[], upstream: string, linted: boolean, exec: string): Verbs => {
    const metadata = (description: string, example: readonly string[]) => ({ description, help: { command: `${exec} tsc --help`, example: { args: [...example] } } });
    const tsc = {
        executor: 'nx:run-commands',
        options: { commands, cwd: root },
        dependsOn: ['codegen--*', '^codegen'],
        cache: true,
        inputs: ['default', upstream, ...sources.map((at) => join('{workspaceRoot}', at)), { dependentTasksOutputFiles: '**/*', transitive: true }, { externalDependencies: [...packages] }],
        outputs: [],
    } satisfies TargetConfiguration;
    return Array.match(outputs, {
        onEmpty: () => ({ check: { tsc: { ...tsc, metadata: metadata('Type-checks every program of the project with tsc', ['--extendedDiagnostics']) } } }),
        onNonEmpty: (written) => {
            const build = { tsc: { ...tsc, outputs: written.map((at) => join('{workspaceRoot}', at)), metadata: metadata('Emits JavaScript and declarations with tsc, type-checking every program it builds', ['--listEmittedFiles']) } };
            const removed = Record.collect(build, (tool) => `build--${tool}`);
            return {
                build,
                check: {
                    tsc: { executor: 'nx:noop', dependsOn: ['build--tsc'], metadata: metadata('Type-checks through the build--tsc compile with tsc', ['--extendedDiagnostics']) },
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
                        metadata: { description: `Removes the outputs ${removed.join(', ')} declare and each program output with tsc --build --clean`, help: { command: `${exec} tsc --build --help`, example: { args: ['--dry'] } } },
                    },
                },
            };
        },
    });
};

const program = Effect.fnUntraced(function* (cache: PluginCache<Program>, context: CreateNodesContext, packages: readonly string[], key: string, fresh: Option.Option<Result.Result<Solution, Cause.UnknownError>>, config: string) {
    const root = dirname(config);
    const read = yield* Option.match(fresh, {
        onNone: () => Effect.mapError(Effect.fromNullishOr(cache.get(key)), (cause) => new Cause.UnknownError(cause, `${config} holds no cached program and no fresh read`)),
        onSome: (solution) => Effect.flatMap(Effect.fromResult(solution), (held) => resolved(context.workspaceRoot, held)),
    });
    cache.set(key, read);
    const { exec } = getPackageManagerCommand(detectPackageManager(context.workspaceRoot));
    const upstream = 'production' in getNamedInputs(root, context) ? '^production' : '^default';
    return yield* projected(context.workspaceRoot, root, 'typescript', compiled(root, read, packages, upstream, installed('publint', join(context.workspaceRoot, config)), exec));
});

const tarball = Effect.fnUntraced(function* ({ workspaceRoot }: CreateNodesContext, { outputPath, releaseTag, platformTagPrefix }: typeof Options.Type, manifest: string) {
    const fs = yield* FileSystem.FileSystem;
    const root = dirname(manifest);
    const { version, os } = yield* Effect.flatMap(fs.readFileString(join(workspaceRoot, manifest)), Schema.decodeEffect(Manifest));
    const manager = detectPackageManager(workspaceRoot);
    const { argv, to, workspace } = PACKS[manager];
    const folder = Option.fromUndefinedOr(outputPath);
    const pack = {
        [manager]: {
            command: [...argv, ...Option.match(folder, { onNone: () => [], onSome: (at) => to(relative(root, at)) })].map(quoteShellArg).join(' '),
            options: { cwd: root },
            dependsOn: ['build--*'],
            cache: true,
            inputs: ['default', join('{workspaceRoot}', workspace), { dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }],
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
        clean: { [manager]: { executor: `${distribution.name}:clean`, options: { targets: removed }, metadata: { description: `Removes the outputs ${removed.join(', ')} declare`, help: { command: `${getPackageManagerCommand(manager).exec} nx run --help`, example: { options: { targets: removed } } } } } },
        attach: {
            gh: {
                executor: `${distribution.name}:attach`,
                dependsOn: removed,
                inputs: [{ dependentTasksOutputFiles: '**/*' }],
                metadata: { description: `Uploads the files ${removed.join(', ')} wrote to the GitHub release of the version the project's manifest states with gh release upload`, help: { command: 'gh release upload --help', example: { options: { dryRun: true } } } },
            },
        },
    };
    const systems = UndefinedOr.match(platformTagPrefix, { onUndefined: () => [], onDefined: (prefix) => os.filter((system) => !system.startsWith('!')).map((system) => `${prefix}${system}`) });
    return yield* UndefinedOr.match(version, {
        onUndefined: () => Effect.succeed(Option.map(Option.liftPredicate(systems, Array.isReadonlyArrayNonEmpty), (tags): ProjectConfiguration => ({ root, tags: [...tags] }))),
        onDefined: () => Effect.asSome(projected(workspaceRoot, root, 'npm', verbs, { tags: [...Array.fromNullishOr(releaseTag), ...systems], release: {} })),
    });
});

const worker = Effect.fnUntraced(function* (context: CreateNodesContext, file: string) {
    const { exec } = getPackageManagerCommand(detectPackageManager(context.workspaceRoot));
    const [root, at] = [dirname(file), join(context.workspaceRoot, file)];
    const { loadConfig }: typeof import('@cloudflare/config') = yield* Effect.tryPromise({
        try: () => import(import.meta.resolve('@cloudflare/config', pathToFileURL(createRequire(at).resolve('cf/package.json')))),
        catch: (cause) => new Cause.UnknownError(cause, `${file} resolves no @cloudflare/config`),
    });
    const { dependencies } = yield* Effect.tryPromise({ try: () => loadConfig(at), catch: (cause) => new Cause.UnknownError(cause, `${file} does not load as a Cloudflare config`) });
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
    return yield* projected(context.workspaceRoot, root, 'cloudflare', {
        codegen,
        clean: { cf: { executor: `${distribution.name}:clean`, options: { targets: removed }, metadata: { description: `Removes the outputs ${removed.join(', ')} declare`, help: { command: `${exec} nx run --help`, example: { options: { targets: removed } } } } } },
    });
});

const nodes = Effect.fnUntraced(function* (files: readonly string[], options: typeof Options.Encoded | undefined, context: CreateNodesContext) {
    const settings = yield* Schema.decodeEffect(Options)(options ?? {});
    const held = new Set(files.filter((file) => basename(file) === MANIFEST && dirname(file) !== '.').map(dirname));
    const nested = files.filter((file) => held.has(dirname(file)));
    const configs = nested.filter((file) => basename(file) === CONFIG);
    const cache = new PluginCache<Program>(join(workspaceDataDirectory, `typescript-${hashObject(options ?? {})}.hash`));
    const lock = getLockFileName(detectPackageManager(context.workspaceRoot));
    const hashes = yield* Effect.promise(() =>
        calculateHashesForCreateNodes(
            configs.map(dirname),
            settings,
            context,
            configs.map(() => [lock]),
        ),
    );
    const keys = new Map(Array.zip(configs, hashes).map(([config, hash]) => [config, `${hash}${config}`] as const));
    const kept = yield* Effect.filter(configs, (config) => UndefinedOr.match(cache.get(keys.get(config) ?? config), { onUndefined: () => Effect.succeed(false), onDefined: ({ sources, stamp }) => Effect.map(stamped(context.workspaceRoot, sources), Equal.equals(stamp)) }));
    const solutions = new Map(yield* Array.match(Array.difference(configs, kept), { onEmpty: () => Effect.succeed([]), onNonEmpty: (unread) => solved(context.workspaceRoot, Array.headNonEmpty(settings.compilerPackages), unread) }));
    const read = (file: string): Effect.Effect<Option.Option<ProjectConfiguration>, Cause.UnknownError | PlatformError.PlatformError | Schema.SchemaError, FileSystem.FileSystem> =>
        Match.value(basename(file)).pipe(
            Match.when(CONFIG, () => Effect.asSome(program(cache, context, settings.compilerPackages, keys.get(file) ?? file, Option.fromUndefinedOr(solutions.get(file)), file))),
            Match.when(WORKER, () =>
                Effect.when(
                    worker(context, file),
                    Effect.sync(() => installed('cf', join(context.workspaceRoot, file))),
                ),
            ),
            Match.orElse(() => tarball(context, settings, file)),
        );
    const each = (file: string) =>
        run(
            Effect.catchCause(
                Effect.map(read(file), (project) => ({ projects: Record.getSomes({ [dirname(file)]: project }) })),
                rendered,
            ),
        );
    return yield* Effect.ensuring(
        Effect.tryPromise({
            try: () => createNodesFromFiles(each, nested, options, context),
            catch: (cause) => (cause instanceof AggregateCreateNodesError ? cause : new AggregateCreateNodesError([[null, new Cause.UnknownError(cause, `${PLUGIN} infers no project`)]], [])),
        }),
        Effect.sync(() => cache.writeToDisk()),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = [`**/{${CONFIG},${MANIFEST},${WORKER}}`, (files, options, context) => run(nodes(files, options, context))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
