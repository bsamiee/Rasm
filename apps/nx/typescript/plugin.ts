import { createRequire, findPackageJSON } from 'node:module';
import { basename, dirname, extname, join, relative, resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesContext, createNodesFromFiles, detectPackageManager, getPackageManagerCommand, normalizePath, type ProjectConfiguration, type TargetConfiguration } from '@nx/devkit';
import { calculateHashesForCreateNodes, getLockFileName, getNamedInputs, hashObject, hashWithWorkspaceContext, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import type { ParsedCommandLine } from '@typescript/native/sync';
import { Array, Cause, Effect, Equal, FileSystem, Match, Option, type PlatformError, Predicate, Record, Result, Schema, UndefinedOr } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Solution = Result.Result.Success<Effect.Success<ReturnType<typeof solved>>[number][1]>;
type Program = Effect.Success<ReturnType<typeof resolved>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const CONFIG = 'tsconfig.json';
const MANIFEST = 'package.json';
const WORKER = 'cloudflare.config.ts';
const SHARED = distribution.name;

// --- [MODELS] --------------------------------------------------------------------------

const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.String, version: Schema.optionalKey(Schema.String), os: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) }));
const Options = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String), compilerPackages: Schema.NonEmptyArray(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed(['typescript'] as const))) }).annotate({
    description: 'compilerPackages: npm packages every tsc target hashes as external dependencies, the first the native TypeScript package whose tsc runs and whose ./sync API reads each tsconfig.json',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const refused = (subject: string, detail: string) => (cause?: unknown) => new Cause.UnknownError(cause, `${subject}: ${detail}`);

const run = <A, E>(effect: Effect.Effect<A, E, FileSystem.FileSystem>) => Effect.runPromise(Effect.provide(effect, layer));

const installed = (name: string, from: string) => Option.isSome(Option.flatMap(Option.liftThrowable(findPackageJSON)(name, pathToFileURL(from)), Option.fromUndefinedOr));

const stamped = (workspaceRoot: string, sources: readonly string[]) => Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, Array.dedupe(sources.flatMap((at) => [at, join(dirname(at), '*')]))));

const solved = Effect.fnUntraced(function* (workspaceRoot: string, compiler: string, configs: Array.NonEmptyReadonlyArray<string>) {
    const unopened = refused(workspaceRoot, 'opens no TypeScript project');
    const { API }: typeof import('@typescript/native/sync') = yield* Effect.tryPromise({
        try: () => import(pathToFileURL(createRequire(resolve(workspaceRoot, Array.headNonEmpty(configs))).resolve(`${compiler}/sync`)).href),
        catch: refused(compiler, 'exports no ./sync compiler API, which native TypeScript holds'),
    });
    const api = yield* Effect.acquireDisposable(Effect.sync(() => new API({ cwd: workspaceRoot })));
    const referenced = ({ projectReferences = [] }: ParsedCommandLine) => projectReferences.map(({ path: at }) => (extname(at) === '.json' ? at : join(at, CONFIG)));
    const parsed = (config: string) => {
        const file = resolve(workspaceRoot, config);
        const read = api.parseConfigFile(file);
        return { errors: read.errors.map(({ text }) => text), owned: [file, ...referenced(read)] };
    };
    const reads = configs.map((config) => [config, Result.try({ try: () => parsed(config), catch: refused(config, 'does not read as a TypeScript configuration') })] as const);
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
            const stated = resolve(options.outDir ?? dirname(file), options.outDir === undefined || options.rootDir === undefined ? '.' : relative(options.rootDir, dirname(file)));
            return [
                join(options.outDir ?? dirname(file), emitted),
                ...(options.declarationDir === undefined ? [] : [join(options.declarationDir, '**/*.{d.ts,d.cts,d.mts}{,.map}')]),
                ...(built || options.incremental === true || options.composite === true ? [options.tsBuildInfoFile ?? join(stated, `${basename(file, '.json')}.tsbuildinfo`)] : []),
            ].map(local);
        });
    const sources = Array.dedupe(holding.flatMap(({ chain, parsed }) => [...chain, ...parsed.fileNames]).map(local)).filter((at) => !(at.startsWith('..') || at.startsWith(`${dirname(config)}/`) || at.split('/').includes('node_modules')));
    yield* Array.match(errors, { onEmpty: () => Effect.void, onNonEmpty: (reasons) => Effect.fail(refused(config, reasons.join('; '))()) });
    return {
        commands: checked.map(({ file, parsed }) => ['tsc', (parsed.projectReferences ?? []).length > 0 ? '--build' : '--project', relative(folder, file)].map(quoteShellArg).join(' ')),
        configs: checked.map(({ file }) => relative(folder, file)),
        outputs: Array.dedupe(outputs),
        sources,
        stamp: yield* stamped(workspaceRoot, sources),
    };
});

const projected = (root: string, technology: string, targets: Readonly<Record<string, TargetConfiguration>>, stated: Partial<ProjectConfiguration> = {}): ProjectConfiguration => ({
    root,
    metadata: { technologies: [technology] },
    ...stated,
    targets: {
        ...targets,
        ...Record.map(
            Array.groupBy(Object.entries(targets), ([label]) => label.slice(0, label.indexOf('--'))),
            (members, verb): TargetConfiguration => ({ dependsOn: [`${verb}--*`], ...Array.match(Array.dedupe(members.flatMap(([, { inputs: hashed = [] }]) => hashed)), { onEmpty: () => ({}), onNonEmpty: (unique) => ({ inputs: ['...', ...unique] }) }) }),
        ),
    },
});

const compiled = (root: string, { commands, configs, outputs, sources }: Program, packages: readonly string[], upstream: string, linted: boolean, exec: string): Record<string, TargetConfiguration> => {
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
        onEmpty: () => ({ 'check--tsc': { ...tsc, metadata: metadata('Type-checks every program of the project with tsc', ['--extendedDiagnostics']) } }),
        onNonEmpty: (written) => ({
            'build--tsc': { ...tsc, outputs: written.map((at) => join('{workspaceRoot}', at)), metadata: metadata('Emits JavaScript and declarations with tsc, type-checking every program it builds', ['--listEmittedFiles']) },
            'check--tsc': { executor: 'nx:noop', dependsOn: ['build--tsc'], metadata: metadata('Type-checks through the build--tsc compile', ['--extendedDiagnostics']) },
            ...(linted && {
                'check--publint': {
                    command: 'publint --strict',
                    options: { cwd: root },
                    dependsOn: ['build--*'],
                    cache: true,
                    inputs: ['default', { dependentTasksOutputFiles: '**/*' }, { externalDependencies: ['publint'] }],
                    outputs: [],
                    metadata: { description: 'Lints the tarball the package manager packs against its package.json with publint, warnings failing through --strict', help: { command: `${exec} publint run --help`, example: { args: ['--level', 'warning'] } } },
                },
            }),
            'clean--tsc': {
                executor: `${SHARED}:clean`,
                options: { targets: ['build--tsc'], commands: configs.map((config) => ['tsc', '--build', '--clean', config]), cwd: root },
                metadata: { description: 'Removes the outputs build--tsc declares and each program output tsc --build --clean deletes', help: { command: `${exec} tsc --build --help`, example: { args: ['--dry'] } } },
            },
        }),
    });
};

const program = Effect.fnUntraced(function* (cache: PluginCache<Program>, context: CreateNodesContext, packages: readonly string[], key: string, fresh: Option.Option<Result.Result<Solution, Cause.UnknownError>>, config: string) {
    const read = yield* Option.match(fresh, {
        onNone: () => Effect.mapError(Effect.fromNullishOr(cache.get(key)), refused(config, 'opens no TypeScript project')),
        onSome: (solution) => Effect.flatMap(Effect.fromResult(solution), (held) => resolved(context.workspaceRoot, held)),
    });
    cache.set(key, read);
    const { exec } = getPackageManagerCommand(detectPackageManager(context.workspaceRoot));
    const upstream = 'production' in getNamedInputs(dirname(config), context) ? '^production' : '^default';
    return projected(dirname(config), 'typescript', compiled(dirname(config), read, packages, upstream, installed('publint', join(context.workspaceRoot, config)), exec));
});

const tarball = Effect.fnUntraced(function* ({ workspaceRoot }: CreateNodesContext, placed: Option.Option<string>, manifest: string) {
    const fs = yield* FileSystem.FileSystem;
    const root = dirname(manifest);
    const { name, version, os } = yield* Effect.flatMap(fs.readFileString(join(workspaceRoot, manifest)), Schema.decodeEffect(Manifest));
    const folder = Option.match(placed, { onNone: () => root, onSome: (at) => join(at, 'pnpm', name) });
    const packed: Record<string, TargetConfiguration> = {
        'pack--pnpm': {
            command: ['pnpm', 'pack', '--out', join(relative(root, folder), '%s.tgz')].map(quoteShellArg).join(' '),
            options: { cwd: root },
            dependsOn: ['build--*'],
            cache: true,
            inputs: ['default', '{workspaceRoot}/pnpm-workspace.yaml', { dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }],
            outputs: [join('{workspaceRoot}', folder, '*.tgz')],
            metadata: { description: 'Packs the package into one tarball named for it with pnpm pack', help: { command: 'pnpm pack --help', example: { args: ['--json'] } } },
        },
        'clean--pnpm': { executor: `${SHARED}:clean`, options: { targets: ['pack--pnpm'] }, metadata: { description: 'Removes the outputs pack--pnpm declares' } },
        'attach--gh': { executor: `${SHARED}:attach`, dependsOn: ['pack--pnpm'], inputs: [{ dependentTasksOutputFiles: '**/*', transitive: true }], metadata: { description: "Uploads the files pack--pnpm wrote to the GitHub release of the version the project's manifest states through gh release upload" } },
    };
    const systems = os.filter((system) => !system.startsWith('!')).map((system) => `os:${system}`);
    return UndefinedOr.match(version, {
        onUndefined: () => Option.map(Option.liftPredicate(systems, Array.isReadonlyArrayNonEmpty), (tags): ProjectConfiguration => ({ root, tags: [...tags] })),
        onDefined: () => Option.some(projected(root, 'npm', packed, { tags: ['release', ...systems], release: {} })),
    });
});

const worker = Effect.fnUntraced(function* (context: CreateNodesContext, file: string) {
    const { exec } = getPackageManagerCommand(detectPackageManager(context.workspaceRoot));
    const [root, own] = [dirname(file), join(context.workspaceRoot, file)];
    const { loadConfig }: typeof import('@cloudflare/config') = yield* Effect.tryPromise({ try: () => import(import.meta.resolve('@cloudflare/config', pathToFileURL(createRequire(own).resolve('cf/package.json')))), catch: refused(file, 'resolves no @cloudflare/config through cf') });
    const { dependencies } = yield* Effect.tryPromise({ try: () => loadConfig(own), catch: refused(file, 'does not load as a Cloudflare config') });
    const sources = Array.map(Array.fromIterable(dependencies), (at) => normalizePath(relative(context.workspaceRoot, at))).filter((at) => !(at.startsWith('..') || at.startsWith(`${root}/`) || at.split('/').includes('node_modules')));
    const typed = installed('@cloudflare/workers-types', own);
    const targets: Record<string, TargetConfiguration> = {
        'codegen--cf': {
            command: typed ? 'cf workers types --include-runtime=false' : 'cf workers types',
            options: { cwd: root },
            dependsOn: ['^codegen'],
            cache: true,
            inputs: ['default', 'production' in getNamedInputs(root, context) ? '^production' : '^default', ...sources.map((at) => join('{workspaceRoot}', at)), { externalDependencies: ['cf'] }],
            outputs: ['{projectRoot}/.cloudflare/types'],
            metadata: {
                description: typed ? `Generates the Worker binding types from ${WORKER}, runtime types coming from @cloudflare/workers-types` : `Generates the Worker binding and runtime types from ${WORKER}`,
                help: { command: `${exec} cf workers types --help`, example: { args: ['--mode', 'production'] } },
            },
        },
        'clean--cf': { executor: `${SHARED}:clean`, options: { targets: ['codegen--cf'] }, metadata: { description: 'Removes the outputs codegen--cf declares' } },
    };
    return projected(root, 'cloudflare', targets);
});

const nodes = Effect.fnUntraced(function* (files: readonly string[], options: unknown, context: CreateNodesContext) {
    const { compilerPackages, outputPath } = yield* Schema.decodeEffect(Options)({ ...context.nxJsonConfiguration.pluginsConfig?.[SHARED], ...(Predicate.isObject(options) ? options : {}) });
    const held = new Set(files.filter((file) => basename(file) === MANIFEST && dirname(file) !== '.').map(dirname));
    const nested = files.filter((file) => held.has(dirname(file)));
    const configs = nested.filter((file) => basename(file) === CONFIG);
    const cache = new PluginCache<Program>(join(workspaceDataDirectory, `typescript-${hashObject({ options })}.hash`));
    const lock = getLockFileName(detectPackageManager(context.workspaceRoot));
    const hashes = yield* Effect.promise(() =>
        calculateHashesForCreateNodes(
            configs.map(dirname),
            { options },
            context,
            configs.map(() => [lock]),
        ),
    );
    const keys = new Map(Array.zip(configs, hashes).map(([config, hash]) => [config, `${hash}_${config}`] as const));
    const kept = yield* Effect.filter(configs, (config) => UndefinedOr.match(cache.get(keys.get(config) ?? config), { onUndefined: () => Effect.succeed(false), onDefined: ({ sources, stamp }) => Effect.map(stamped(context.workspaceRoot, sources), Equal.equals(stamp)) }));
    const solutions = new Map(yield* Array.match(Array.difference(configs, kept), { onEmpty: () => Effect.succeed([]), onNonEmpty: (unread) => solved(context.workspaceRoot, Array.headNonEmpty(compilerPackages), unread) }));
    const read = (file: string): Effect.Effect<Option.Option<ProjectConfiguration>, Cause.UnknownError | PlatformError.PlatformError | Schema.SchemaError, FileSystem.FileSystem> =>
        Match.value(basename(file)).pipe(
            Match.when(CONFIG, () => Effect.asSome(program(cache, context, compilerPackages, keys.get(file) ?? file, Option.fromUndefinedOr(solutions.get(file)), file))),
            Match.when(WORKER, () =>
                Effect.when(
                    worker(context, file),
                    Effect.sync(() => installed('cf', join(context.workspaceRoot, file))),
                ),
            ),
            Match.orElse(() => tarball(context, Option.fromUndefinedOr(outputPath), file)),
        );
    const each = (file: string) => run(Effect.map(read(file), (project) => ({ projects: Record.getSomes({ [dirname(file)]: project }) })));
    return yield* Effect.ensuring(
        Effect.tryPromise({ try: () => createNodesFromFiles(each, nested, options, context), catch: (cause) => (cause instanceof AggregateCreateNodesError ? cause : new AggregateCreateNodesError([[null, new Cause.UnknownError(cause, 'createNodes failed outside a file')]], [])) }),
        Effect.sync(() => cache.writeToDisk()),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [`**/{${CONFIG},${MANIFEST},${WORKER}}`, (files, options, context) => run(nodes(files, options, context))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
