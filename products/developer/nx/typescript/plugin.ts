import { createRequire, findPackageJSON } from 'node:module';
import { basename, dirname, extname, join, posix, relative, resolve } from 'node:path';
import { execPath } from 'node:process';
import { fileURLToPath, pathToFileURL } from 'node:url';
import type { loadConfig } from '@cloudflare/config';
import { type CreateNodes, type CreateNodesContext, createNodesFromFiles, detectPackageManager, getPackageManagerCommand, hashArray, normalizePath, type PackageManager, type ProjectConfiguration, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { buildPackageJsonPatterns, buildPackageJsonWorkspacesMatcher, getFilesInDirectoryUsingContext, getLockFileName, hashObject, hashWithWorkspaceContext, type PackageJson, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { getRootTsConfigFileName } from '@nx/js';
import { aggregated, fingerprinted, Folder, inferring, namedInput, recalled, run, spawned, type Stated, stamped, tagged, Targets, tokened, type Verbs, within } from '@rasm/nx-distribution/inference';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import type { API, ParsedCommandLine } from '@typescript/native/sync';
import { Array, Cause, Effect, FileSystem, Filter, Match, Option, Order, Predicate, Record, Result, Schema, type Scope, String, UndefinedOr } from 'effect';
import type { ESLint } from 'eslint';

// --- [TYPES] ---------------------------------------------------------------------------

type Program = Effect.Success<ReturnType<typeof resolved>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const CONFIG = 'tsconfig.json';
const MANIFEST = 'package.json';
const WORKER = 'cloudflare.config.ts';
const PROJECT = 'project.json';
const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#/package.json'))).name;
const LINTERS = ['eslint.config.js', 'eslint.config.mjs', 'eslint.config.cjs', 'eslint.config.ts', 'eslint.config.mts', 'eslint.config.cts'];
const DUPLICATES = ['.jscpd.json', '.config/jscpd.json', '.config/.jscpd.json'];
const PACKAGE = /^(?![./]|node:)(?<name>(?:@[^/]+\/)?[^/]+)/u;
const UNPACKAGED: NonNullable<TargetConfiguration['inputs']> = ['default', '^default', { externalDependencies: [] }];
const MANAGERS = {
    npm: { argv: ['npm', 'pack'], to: (at: string) => ['--pack-destination', at], workspace: { json: posix.join('{workspaceRoot}', MANIFEST), fields: ['workspaces'] }, catalogs: [] },
    pnpm: { argv: ['pnpm', 'pack'], to: (at: string) => ['--out', posix.join(at, '%s.tgz')], workspace: posix.join('{workspaceRoot}', 'pnpm-workspace.yaml'), catalogs: ['pnpm-workspace.yaml'] },
    yarn: { argv: ['yarn', 'pack'], to: (at: string) => ['--out', posix.join(at, '%s.tgz')], workspace: posix.join('{workspaceRoot}', '.yarnrc.yml'), catalogs: ['.yarnrc.yml'] },
    bun: { argv: ['bun', 'pm', 'pack'], to: (at: string) => ['--destination', at], workspace: { json: posix.join('{workspaceRoot}', MANIFEST), fields: ['workspaces'] }, catalogs: [] },
} as const satisfies Record<PackageManager, { readonly argv: readonly string[]; readonly to: (at: string) => readonly string[]; readonly workspace: NonNullable<TargetConfiguration['inputs']>[number]; readonly catalogs: readonly string[] }>;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({
    outputPath: Folder,
    compilerPackages: Schema.NonEmptyArray(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed(['typescript'] as const))),
    knipConfig: Folder,
    stylelintConfig: Folder,
}).annotate({
    description:
        'outputPath: workspace folder the package manager packs each tarball to, {projectName} naming the project, absent packs into the package folder. compilerPackages: npm packages every tsc target hashes as external dependencies, the first the native TypeScript package whose tsc runs and whose ./sync API reads each tsconfig.json. knipConfig: workspace file knip reads its configuration from through --config, absent leaving knip to find its own. stylelintConfig: workspace module whose default export stylelint reads as its configuration through --config over every .css file Git lists, absent inferring no check--stylelint',
});
class Manifest extends Schema.Opaque<Manifest, unknown>()(
    Schema.fromJsonString(
        Schema.Struct({
            version: Schema.optionalKey(Schema.String),
            os: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
            nx: Schema.optionalKey(Schema.Struct({ targets: Schema.optionalKey(Targets) })),
            jscpd: Schema.optionalKey(Schema.Unknown),
        }),
    ).annotate({ description: "The version, os, nx.targets, and jscpd rows of a project's package.json, Nx merging the inputs over the inferred targets after every plugin and jscpd reading its configuration from the jscpd row" }),
) {}
const Literal = Schema.Struct({ text: Schema.String }).annotate({ description: 'A module specifier literal of a source file the native TypeScript API parses' });
const Exported = Schema.fromJsonString(Schema.Struct({ exports: Schema.Struct({ '.': Schema.Struct({ import: Schema.String }) }) })).annotate({ description: 'The import entry of the root export of @cloudflare/config, the package cf loads cloudflare.config.ts through' });
const Scanned = Schema.fromJsonString(Schema.Struct({ pattern: Schema.NullOr(Schema.String) })).annotate({ description: 'The pattern row of the merged configuration jscpd --debug prints, the glob of the files jscpd scans, null scanning every file' });
const Lookups = Schema.Struct({
    extends: Schema.optionalKey(Schema.Unknown),
    plugins: Schema.optionalKey(Schema.Unknown),
    processors: Schema.optionalKey(Schema.Unknown),
    customSyntax: Schema.optionalKey(Schema.Unknown),
    referenceFiles: Schema.ArrayEnsure(Schema.Union([Schema.String, Schema.Struct({ files: Schema.ArrayEnsure(Schema.String), customSyntax: Schema.optionalKey(Schema.Unknown), loader: Schema.optionalKey(Schema.Unknown) })])).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
}).annotate({ description: 'The rows of a Stylelint config or of one overrides entry naming modules and reference files, each one entry or many: a string names a module, any other value an inline one, and a referenceFiles entry is a glob or a files row of globs with its customSyntax and loader' });
const Styled = Schema.fromJsonString(Schema.Struct({ default: Schema.Struct({ ...Lookups.fields, overrides: Schema.Array(Lookups).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) }) })).annotate({
    description: 'The default export of a Stylelint config module as JSON, the rows naming modules and reference files at its top and in each overrides entry',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const own = fingerprinted(import.meta.dirname);

const manifested = Effect.fnUntraced(function* (workspaceRoot: string, root: string) {
    const fs = yield* FileSystem.FileSystem;
    const at = join(workspaceRoot, root, MANIFEST);
    return [
        root,
        yield* Effect.result(
            Effect.flatMap(
                Effect.flatMap(fs.exists(at), (present) => (present ? fs.readFileString(at) : Effect.succeed('{}'))),
                Schema.decodeEffect(Manifest),
            ),
        ),
    ] as const;
});

const projected = (root: string, technology: string, verbs: Verbs, stated: Stated, extra: Partial<ProjectConfiguration> = {}) => ({ root, metadata: { technologies: [technology] }, ...extra, targets: aggregated(verbs, stated, technology) }) satisfies ProjectConfiguration;

const installed = (name: string, from: string) => Option.isSome(Option.flatMap(Option.liftThrowable(findPackageJSON)(name, pathToFileURL(from)), Option.fromUndefinedOr));

const listing = (workspaceRoot: string) => (sources: readonly string[]) => stamped(workspaceRoot)(Array.dedupe(sources.flatMap((at) => [at, posix.join(dirname(at), '*')])));

const kept = Effect.fnUntraced(function* (cache: PluginCache<Program & { readonly digest: string }>, workspaceRoot: string, lock: string, row: string, config: string) {
    const root = dirname(config);
    const [configured, listed] = yield* Effect.all([Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [posix.join(root, '*.json'), posix.join(root, '**/tsconfig*.json'), lock])), Effect.promise(() => getFilesInDirectoryUsingContext(workspaceRoot, root))], { concurrency: 'unbounded' });
    const digest = hashArray([row, configured, ...listed]);
    const program = Option.filter(yield* recalled(cache, config, listing(workspaceRoot)), (held) => held.digest === digest);
    return { config, digest, program };
});

const referenced = ({ projectReferences = [] }: ParsedCommandLine) => projectReferences.map(({ path: at }) => (extname(at) === '.json' ? at : join(at, CONFIG)));

const owning = (workspaceRoot: string, roots: ReadonlySet<string>) => (at: string) => {
    const parts = normalizePath(relative(workspaceRoot, dirname(at))).split('/');
    return Option.getOrElse(
        Array.findFirst(
            Array.makeBy(parts.length, (index) => parts.slice(0, parts.length - index).join('/')),
            (folder) => roots.has(folder),
        ),
        () => '.',
    );
};

const connected = (workspaceRoot: string, compiler: string, from: string) =>
    Effect.flatMap(
        Effect.tryPromise({
            try: (): Promise<{ readonly API: typeof API }> => import(pathToFileURL(createRequire(resolve(workspaceRoot, from)).resolve(`${compiler}/sync`)).href),
            catch: (cause) => new Cause.UnknownError(cause, `${workspaceRoot} resolves no ${compiler}/sync`),
        }),
        (native) => Effect.acquireDisposable(Effect.sync(() => new native.API({ cwd: workspaceRoot }))),
    );

const solved = Effect.fnUntraced(function* (workspaceRoot: string, compiler: string, roots: ReadonlySet<string>, configs: Array.NonEmptyReadonlyArray<string>) {
    const unopened = (cause: unknown) => new Cause.UnknownError(cause, `${compiler}/sync opens no snapshot of ${workspaceRoot}`);
    const api = yield* connected(workspaceRoot, compiler, Array.headNonEmpty(configs));
    const fs = yield* FileSystem.FileSystem;
    const linked = (at: string) =>
        Result.getOrElse(
            Result.try(() => referenced(api.parseConfigFile(at))),
            () => [],
        );
    const reached = (pending: readonly string[], seen: ReadonlySet<string>): ReadonlySet<string> =>
        Array.match(
            pending.filter((at) => !seen.has(at)),
            { onEmpty: () => seen, onNonEmpty: (fresh) => reached(fresh.flatMap(linked), new Set([...seen, ...fresh])) },
        );
    const owner = owning(workspaceRoot, roots);
    const parsed = (config: string) => {
        const file = resolve(workspaceRoot, config);
        const read = api.parseConfigFile(file);
        const [owned, external] = Array.partition(
            referenced(read),
            Filter.fromPredicate((at: string) => owner(at) === dirname(config)),
        );
        return { entry: { file, parsed: read, chain: [file], loaded: read.fileNames }, owned, external: [...reached([...(Array.isReadonlyArrayEmpty(read.fileNames) ? [] : external), ...owned.flatMap(linked)], new Set([file, ...owned]))].filter((at) => !(at === file || owned.includes(at))) };
    };
    const reads = configs.map((config) => [config, Result.try({ try: () => parsed(config), catch: (cause) => new Cause.UnknownError(cause, `${config} does not parse as a tsconfig`) })] as const);
    const files = yield* Effect.filter(Array.dedupe(Array.getSuccesses(reads.map(([, read]) => read)).flatMap(({ entry, owned, external }) => [entry.file, ...owned, ...external])), (at) => fs.exists(at));
    const snapshot = yield* Effect.acquireDisposable(Effect.try({ try: () => api.createSnapshot({ openProjects: files }), catch: unopened }));
    const configured = (file: string) => Option.map(Option.fromNullishOr(snapshot.getConfiguredProject(file)), (project) => [file, { file, parsed: project.parsedCommandLine, chain: project.program.getConfigFileNames(), loaded: project.program.getSourceFileNames() }] as const);
    const opened = new Map(yield* Effect.try({ try: () => Array.getSomes(files.map(configured)), catch: unopened }));
    const members = (listed: readonly string[]) => Array.getSomes(listed.map((file) => Option.fromUndefinedOr(opened.get(file))));
    return reads.map(([config, read]) => [config, Result.map(read, ({ entry, owned, external }) => ({ config, entry: opened.get(entry.file) ?? entry, members: members(owned), external: members(external) }))] as const);
}, Effect.scoped);

const resolved = Effect.fnUntraced(function* (workspaceRoot: string, roots: ReadonlySet<string>, solution: Effect.Success<ReturnType<typeof solved>>[number][1]) {
    const { config, entry, members, external } = yield* Effect.fromResult(solution);
    const folder = join(workspaceRoot, dirname(config));
    const local = (at: string) => normalizePath(relative(workspaceRoot, at));
    const holding = [entry, ...members].filter(({ parsed }) => parsed.fileNames.length > 0);
    const checked = entry.parsed.fileNames.length > 0 || (Array.isReadonlyArrayEmpty(holding) && Array.isReadonlyArrayEmpty(entry.parsed.projectReferences ?? [])) ? [entry] : holding;
    const placed = holding.map(({ file, loaded, parsed: { options, projectReferences = [] } }) => {
        const offset = options.outDir === undefined || options.rootDir === undefined ? '.' : relative(options.rootDir, dirname(file));
        const base = join(options.outDir ?? dirname(file), offset.startsWith('..') ? '.' : offset);
        const scripts = options.emitDeclarationOnly === true ? [] : [join(base, `**/*.{js,cjs,mjs,jsx}${options.sourceMap === true ? '{,.map}' : ''}`), ...(loaded.some((at) => extname(at) === '.json') ? [join(base, '**/*.json')] : [])];
        const declarations = options.declaration === true || options.composite === true ? [join(options.declarationDir ?? base, `**/*.{d.ts,d.cts,d.mts}${options.declarationMap === true ? '{,.map}' : ''}`)] : [];
        return {
            emitted: options.noEmit === true ? [] : [...scripts, ...declarations].map(local),
            informed: options.incremental === true || options.composite === true || projectReferences.length > 0 ? [local(options.tsBuildInfoFile ?? join(resolve(options.outDir ?? dirname(file), offset), `${basename(file, '.json')}.tsbuildinfo`))] : [],
        };
    });
    const owner = owning(workspaceRoot, roots);
    const sources = Array.dedupe([...[entry, ...holding].flatMap(({ chain, parsed, loaded }) => [...chain, ...parsed.fileNames, ...loaded.filter((at) => owner(at) === dirname(config))]), ...external.flatMap(({ chain }) => chain)].map(local)).filter(within);
    return {
        commands: checked.map(({ file, parsed }) => {
            const named = relative(folder, file) === CONFIG ? [] : [relative(folder, file)];
            return ['tsc', ...((parsed.projectReferences ?? []).length > 0 ? ['--build', ...named] : named.flatMap((at) => ['-p', at]))].map(quoteShellArg).join(' ');
        }),
        configs: checked.map(({ file }) => relative(folder, file)),
        outputs: Array.dedupe(placed.flatMap(({ emitted }) => emitted)),
        informed: Array.dedupe(placed.flatMap(({ informed }) => informed)),
        chain: entry.chain.map(local).filter(within),
        sources,
        stamp: yield* listing(workspaceRoot)(sources),
    };
});

const compiled = (root: string, { commands, configs, outputs, informed, sources }: Program, packages: readonly string[], upstream: string, linted: boolean, synced: boolean, exec: string): Verbs => {
    const metadata = (description: string, example: readonly string[]) => ({ description, help: { command: `${exec} tsc --help`, example: { args: [...example] } } });
    const token = tokened(root);
    const tsc = {
        executor: 'nx:run-commands',
        options: { commands, cwd: root },
        dependsOn: ['codegen--*', '^codegen--*', '^build--tsc'],
        cache: true,
        inputs: [...Array.dedupe([posix.join(root, MANIFEST), ...sources].map(token)), upstream, { dependentTasksOutputFiles: '**/*.{d.ts,d.cts,d.mts,tsbuildinfo}', transitive: true }, { externalDependencies: [...packages] }],
        outputs: [],
        ...(synced && { syncGenerators: ['@nx/js:typescript-sync'] }),
    } satisfies TargetConfiguration;
    const cleaned = (removed: readonly string[]) => ({
        tsc: {
            executor: `${distribution.name}:clean`,
            options: { targets: removed, commands: configs.map((config) => ['tsc', '--build', '--clean', config]), cwd: root },
            inputs: ['default', '^default', { externalDependencies: [...packages] }],
            metadata: { description: `Removes the outputs ${removed.join(', ')} declare and each program output with tsc --build --clean`, help: { command: `${exec} tsc --build --help`, example: { args: ['--dry'] } } },
        },
    });
    return Array.match(outputs, {
        onEmpty: () => ({ check: { tsc: { ...tsc, outputs: informed.map(token), metadata: metadata('Type-checks every program of the project with tsc', ['--extendedDiagnostics']) } }, ...(Array.isReadonlyArrayNonEmpty(informed) && { clean: cleaned(['check--tsc']) }) }),
        onNonEmpty: (written) => {
            const build = { tsc: { ...tsc, outputs: Array.dedupe([...written, ...informed]).map(token), metadata: metadata('Emits JavaScript and declarations with tsc, type-checking every program it builds', ['--listEmittedFiles']) } };
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
                clean: cleaned(Record.collect(build, (tool) => `build--${tool}`)),
            };
        },
    });
};

const tarball = (manager: PackageManager, exec: string, outputPath: string | undefined, { version, os, nx }: Manifest, root: string, released: readonly string[]) => {
    const { argv, to, workspace } = MANAGERS[manager];
    const folder = Option.fromUndefinedOr(outputPath);
    const pack = {
        [manager]: {
            executor: `${PLUGIN}:pack`,
            options: Option.match(folder, { onNone: () => ({ argv }), onSome: (at) => ({ argv: [...argv, ...to(posix.relative(root, at))], outputPath: at }) }),
            dependsOn: ['build--*'],
            cache: true,
            inputs: ['default', '^{projectRoot}/package.json', workspace, { dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }],
            outputs: [
                tokened(root)(
                    posix.join(
                        Option.getOrElse(folder, () => root),
                        '*.tgz',
                    ),
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
        onDefined: () => Option.some(projected(root, 'npm', verbs, nx?.targets ?? {}, { tags: [...released, ...systems], release: {} })),
    });
};

const workspace = Effect.fnUntraced(function* (workspaceRoot: string, manager: PackageManager, exec: string, knipConfig: string | undefined, { nx, jscpd }: Manifest, stylelint: Option.Option<TargetConfiguration>) {
    const { catalogs } = MANAGERS[manager];
    const knip = ['knip', ...UndefinedOr.match(knipConfig, { onUndefined: () => [], onDefined: (at) => ['--config', at] })].map(quoteShellArg).join(' ');
    const configured = Option.orElse(yield* Effect.findFirst(DUPLICATES, (at) => FileSystem.FileSystem.use((fs) => fs.exists(join(workspaceRoot, at)))), () => Option.liftPredicate(MANIFEST, () => jscpd !== undefined));
    const decoded = Effect.flatMap(spawned(['jscpd', '--debug', '.'], { cwd: workspaceRoot }), Schema.decodeEffect(Scanned));
    const scanned = yield* Effect.transposeOption(Option.as(configured, decoded));
    const duplicated = Option.zipWith(configured, scanned, (config, { pattern }) => ({
        command: 'jscpd .',
        options: { cwd: '.' },
        cache: true,
        inputs: [posix.join('{workspaceRoot}', pattern ?? '**/*'), posix.join('{workspaceRoot}', config), { runtime: 'jscpd --version' }, { externalDependencies: [] }],
        outputs: [],
        metadata: { technologies: ['jscpd'], description: `Reports code duplicated across the workspace files ${config} selects with jscpd`, help: { command: 'jscpd --help', example: { args: ['--changed'] } } },
    }));
    return Option.some(
        projected(
            '.',
            manager,
            {
                upgrade: {
                    [manager]: {
                        executor: `${PLUGIN}:upgrade`,
                        options: { catalogs },
                        inputs: [{ externalDependencies: [] }],
                        metadata: { description: `Moves every exact version of the catalogs and package manifests to the newest dist-tag head published since, then installs and deduplicates the lockfile with ${manager}`, help: { command: `${exec} nx run --help`, example: { options: { dryRun: true } } } },
                    },
                },
                check: {
                    ...Record.getSomes({ jscpd: duplicated, stylelint }),
                    ...(installed('knip', join(workspaceRoot, MANIFEST)) && {
                        knip: {
                            command: knip,
                            options: { cwd: '.' },
                            cache: true,
                            inputs: [posix.join('{workspaceRoot}', '**/*'), { externalDependencies: ['knip'] }],
                            outputs: [],
                            configurations: { ci: { args: '--reporter symbols --reporter github-actions' } },
                            metadata: { technologies: ['knip'], description: `Reports unused files, exports, and dependencies of every workspace package with ${knip}, adding GitHub annotations under the ci configuration`, help: { command: `${exec} knip --help`, example: { args: ['--production'] } } },
                        },
                    }),
                },
            },
            nx?.targets ?? {},
        ),
    );
}, Effect.scoped);

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
            dependsOn: ['^codegen--*'],
            cache: true,
            inputs: ['default', `^${namedInput(root, context)}`, ...sources.map((dependency) => posix.join('{workspaceRoot}', dependency)), { externalDependencies: ['cf'] }],
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

const merged = (prior: Option.Option<ProjectConfiguration>, next: ProjectConfiguration) => Option.some(Option.match(prior, { onNone: () => next, onSome: (held) => ({ ...held, ...next, targets: { ...held.targets, ...next.targets } }) }));

const looked = <A, E>(table: ReadonlyMap<string, Result.Result<A, E>>, at: string) => Effect.flatMap(Effect.fromNullishOr(table.get(at)), Effect.fromResult);

const imported = Effect.fnUntraced(function* (workspaceRoot: string, compiler: string, configs: Array.NonEmptyReadonlyArray<readonly [config: string, named: readonly string[]]>) {
    const api = yield* connected(workspaceRoot, compiler, Array.headNonEmpty(configs)[0]);
    const named = new Map(configs);
    const specified = (file: string) =>
        Effect.map(Effect.acquireDisposable(Effect.try({ try: () => api.createSourceFileFromFile(file), catch: (cause) => new Cause.UnknownError(cause, `${compiler}/sync parses no ${file}`) })), ({ sourceFile }) => {
            const texts = [...(named.get(file) ?? []), ...Array.getSomes(sourceFile.imports.map((node) => Schema.decodeUnknownOption(Literal)(node))).map(({ text }) => text)];
            return { locals: texts.filter((text) => text.startsWith('.')).map((text) => posix.join(dirname(file), text)), packages: Array.getSomes(texts.map((text) => Option.fromNullishOr(PACKAGE.exec(text)?.groups?.['name']))) };
        });
    const reached = (pending: readonly string[], files: ReadonlySet<string>, packages: ReadonlySet<string>): Effect.Effect<{ readonly files: readonly string[]; readonly packages: readonly string[] }, Cause.UnknownError, Scope.Scope> =>
        Array.match(
            pending.filter((at) => !files.has(at)),
            {
                onEmpty: () => Effect.succeed({ files: [...files], packages: [...packages] }),
                onNonEmpty: (fresh) =>
                    Effect.flatMap(Effect.forEach(fresh, specified), (found) =>
                        reached(
                            found.flatMap(({ locals }) => locals),
                            new Set([...files, ...fresh]),
                            new Set([...packages, ...found.flatMap((held) => held.packages)]),
                        ),
                    ),
            },
        );
    return new Map(yield* Effect.forEach(configs, ([config]) => Effect.map(reached([config], new Set(), new Set()), (held) => [config, held] as const)));
}, Effect.scoped);

const inspected = Effect.fnUntraced(function* (workspaceRoot: string, compiler: string, exec: string, rows: Array.NonEmptyReadonlyArray<{ readonly root: string; readonly scope: string; readonly owned: readonly string[] }>) {
    const unanswered = (cause: unknown) => new Cause.UnknownError(cause, `eslint resolved from ${workspaceRoot} answers no config or ignore question`);
    const { ESLint: Linter } = yield* Effect.tryPromise({ try: (): Promise<{ readonly ESLint: typeof ESLint }> => import(pathToFileURL(createRequire(join(workspaceRoot, MANIFEST)).resolve('eslint')).href), catch: unanswered });
    const eslint = new Linter({ cwd: workspaceRoot });
    const linting = (file: string) => Effect.map(Effect.tryPromise({ try: () => eslint.isPathIgnored(join(workspaceRoot, file)), catch: unanswered }), (ignored) => !ignored);
    const governed = yield* Effect.forEach(
        rows,
        Effect.fnUntraced(function* ({ root, scope, owned }) {
            const config = Option.map(Option.fromUndefinedOr(yield* Effect.tryPromise({ try: () => eslint.findConfigFile(join(workspaceRoot, root, MANIFEST)), catch: unanswered })), (at) => normalizePath(relative(workspaceRoot, at)));
            const files = yield* Option.match(config, { onNone: () => Effect.succeed([]), onSome: () => Effect.filter(owned, linting) });
            return { root, scope, config, lintable: Array.isReadonlyArrayNonEmpty(files) };
        }),
        { concurrency: 'unbounded' },
    );
    const imports = yield* Array.match(Array.dedupe(Array.getSomes(governed.map(({ config }) => config))), {
        onEmpty: () => Effect.succeed(new Map<string, { readonly files: readonly string[]; readonly packages: readonly string[] }>()),
        onNonEmpty: (configs) =>
            imported(
                workspaceRoot,
                compiler,
                Array.map(configs, (config) => [config, ['eslint']] as const),
            ),
    });
    const target = (root: string, scope: string, config: string): TargetConfiguration => ({
        command: root === scope ? 'eslint .' : `eslint ./${scope}`,
        options: { cwd: root },
        cache: true,
        inputs: ['default', '^default', ...(imports.get(config)?.files ?? [config]).map(tokened(root)), { externalDependencies: [...(imports.get(config)?.packages ?? ['eslint'])] }],
        outputs: [],
        metadata: { description: `Lints the files ${config} governs in the project with eslint`, help: { command: `${exec} eslint --help`, example: { args: ['--max-warnings', '0'] } } },
    });
    return new Map(
        governed.map(({ root, scope, config, lintable }) => [
            root,
            {
                read: Option.match(config, { onNone: () => [], onSome: (at) => imports.get(at)?.files ?? [at] }),
                targets: Option.toArray(
                    Option.map(
                        Option.filter(config, () => lintable),
                        (at) => target(root, scope, at),
                    ),
                ),
            },
        ]),
    );
});

const linted = Effect.fnUntraced(function* (workspaceRoot: string, compiler: string, exec: string, roots: readonly { readonly root: string; readonly scope: string }[], hashed: string) {
    const cache = new PluginCache<{ readonly targets: readonly TargetConfiguration[]; readonly read: readonly string[]; readonly stamp: string; readonly digest: string }>(join(workspaceDataDirectory, 'typescript-eslint.hash'));
    const lock = getLockFileName(detectPackageManager(workspaceRoot));
    const digested = stamped(workspaceRoot);
    const keyed = yield* Effect.forEach(
        roots,
        Effect.fnUntraced(function* ({ root, scope }: (typeof roots)[number]) {
            const listed = yield* Effect.promise(() => getFilesInDirectoryUsingContext(workspaceRoot, scope));
            const nested = new RegExp(
                `^(?:${roots
                    .filter((other) => other.root.startsWith(`${scope}/`))
                    .map((other) => RegExp.escape(other.root))
                    .join('|')})/`,
                'u',
            );
            const owned = listed.filter((file) => !nested.test(file));
            const parts = root.split('/');
            const ancestors = ['.', ...Array.makeBy(parts.length, (index) => parts.slice(0, index + 1).join('/'))];
            const configured = yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [lock, ...Array.cartesianWith(ancestors, LINTERS, posix.join)]));
            return { root, scope, owned, digest: hashArray([hashed, configured, ...owned]) };
        }),
        { concurrency: 'unbounded' },
    );
    const missed = yield* Effect.filter(keyed, ({ root, digest }) =>
        Option.match(
            Option.filter(Option.fromNullishOr(cache.get(root)), (held) => held.digest === digest),
            {
                onNone: () => Effect.succeed(true),
                onSome: ({ read, stamp }) => Effect.map(digested(read), (now) => now !== stamp),
            },
        ),
    );
    const fresh = yield* Array.match(missed, { onEmpty: () => Effect.succeed(new Map<string, { readonly read: readonly string[]; readonly targets: readonly TargetConfiguration[] }>()), onNonEmpty: (rows) => inspected(workspaceRoot, compiler, exec, rows) });
    yield* Effect.forEach(Array.getSomes(missed.map(({ root, digest }) => Option.map(Option.fromUndefinedOr(fresh.get(root)), (held) => [root, digest, held] as const))), ([root, digest, { read, targets }]) => Effect.map(digested(read), (stamp) => cache.set(root, { targets, read, stamp, digest })));
    yield* Array.isReadonlyArrayNonEmpty(missed) ? Effect.sync(() => cache.writeToDisk()) : Effect.void;
    return new Map(
        Array.getSomes(
            keyed.map(({ root, digest }) =>
                Option.map(
                    Option.flatMap(
                        Option.filter(Option.fromNullishOr(cache.get(root)), (held) => held.digest === digest),
                        ({ targets }) => Array.head(targets),
                    ),
                    (target) => [root, target] as const,
                ),
            ),
        ),
    );
});

const styled = Effect.fnUntraced(function* (workspaceRoot: string, compiler: string, exec: string, row: string, config: string) {
    const cache = new PluginCache<{ readonly sources: readonly string[]; readonly references: readonly string[]; readonly packages: readonly string[]; readonly stamp: string }>(join(workspaceDataDirectory, 'typescript-stylelint.hash'));
    const lock = getLockFileName(detectPackageManager(workspaceRoot));
    const stamp = (sources: readonly string[]) => Effect.map(stamped(workspaceRoot)([lock, ...sources]), (hash) => hashArray([row, hash]));
    const listed = Option.liftPredicate((yield* spawned(['git', 'ls-files', '-z', '--cached', '--others', '--exclude-standard', '--', ':(glob)**/*.css'], { cwd: workspaceRoot })).split('\0').filter(String.isNonEmpty).toSorted(), Array.isReadonlyArrayNonEmpty);
    const read = Effect.flatMap(
        recalled(cache, config, stamp),
        Option.match({
            onSome: Effect.succeed,
            onNone: Effect.fnUntraced(function* () {
                const loaded = spawned([execPath, '--input-type=module', '--eval', 'console.log(JSON.stringify(await import(process.argv[1])))', pathToFileURL(join(workspaceRoot, config)).href], { cwd: workspaceRoot });
                const { default: stated } = yield* Effect.flatMap(loaded, Schema.decodeEffect(Styled));
                const layers = [stated, ...stated.overrides];
                const entries = layers.flatMap(({ referenceFiles }) => referenceFiles);
                const named = [...layers.flatMap(({ extends: bases, plugins, processors, customSyntax }) => [bases, plugins, processors, customSyntax]), ...entries.flatMap((entry) => (Predicate.isString(entry) ? [] : [entry.customSyntax, entry.loader]))].flatMap(Array.ensure).filter(Predicate.isString);
                const walked = yield* Effect.flatMap(imported(workspaceRoot, compiler, [[config, ['stylelint', ...named]]]), (configs) => Effect.fromNullishOr(configs.get(config)));
                const references = entries.flatMap((entry) => (Predicate.isString(entry) ? [entry] : entry.files)).map((pattern) => posix.join(dirname(config), pattern));
                const held = { sources: walked.files, references, packages: walked.packages, stamp: yield* stamp(walked.files) };
                yield* Effect.sync(() => {
                    cache.set(config, held);
                    cache.writeToDisk();
                });
                return held;
            }),
        }),
    );
    return Option.zipWith(listed, yield* Effect.transposeOption(Option.as(listed, read)), (styles, { sources, references, packages }): TargetConfiguration => ({
        command: ['stylelint', '--config', config, '--allow-empty-input', '--no-cache', '--max-warnings', '0', ...styles].map(quoteShellArg).join(' '),
        options: { cwd: '.', env: { NODE_OPTIONS: '--throw-deprecation' } },
        cache: true,
        inputs: [
            ...Array.dedupe([...styles, ...sources, ...references])
                .filter(within)
                .map((at) => posix.join('{workspaceRoot}', at)),
            { externalDependencies: [...packages] },
        ],
        outputs: [],
        metadata: { technologies: ['stylelint'], description: `Lints every style file Git lists in the workspace against ${config} with stylelint`, help: { command: `${exec} stylelint --help`, example: { args: ['--formatter', 'verbose'] } } },
    }));
});

const nodes = inferring(Options, run, function* (files, settings, context) {
    const { workspaceRoot } = context;
    const hashed = yield* own;
    const listed = new Set(files);
    const member = buildPackageJsonWorkspacesMatcher(buildPackageJsonPatterns(workspaceRoot, (file: string) => readJsonFile<PackageJson>(join(workspaceRoot, file))));
    const held = new Set(Array.dedupe(files.map(dirname)).filter((root) => root !== '.' && ((listed.has(posix.join(root, MANIFEST)) && member(posix.join(root, MANIFEST))) || listed.has(posix.join(root, PROJECT)))));
    const nested = files.filter((file) => file === MANIFEST || (file === CONFIG && listed.has(MANIFEST)) || held.has(dirname(file)));
    const anchors = new Set(
        Object.values(
            Array.groupBy(
                nested.filter((file) => held.has(dirname(file))),
                dirname,
            ),
        ).map(Array.min(Order.String)),
    );
    const configs = nested.filter((file) => basename(file) === CONFIG);
    const manager = detectPackageManager(workspaceRoot);
    const { exec } = getPackageManagerCommand(manager);
    const synced = installed('@nx/js', join(workspaceRoot, MANIFEST));
    const row = hashArray([hashed, hashObject(settings)]);
    const cache = new PluginCache<Program & { readonly digest: string }>(join(workspaceDataDirectory, 'typescript.hash'));
    const manifests = new Map(yield* Effect.forEach(Array.dedupe(nested.map(dirname)), (root) => manifested(workspaceRoot, root), { concurrency: 'unbounded' }));
    const rows = yield* Effect.forEach(configs, (config) => kept(cache, workspaceRoot, getLockFileName(manager), row, config), { concurrency: 'unbounded' });
    const unread = rows.filter(({ program }) => Option.isNone(program));
    const solutions = yield* Array.match(unread, {
        onEmpty: () => Effect.succeed([]),
        onNonEmpty: (missed) =>
            solved(
                workspaceRoot,
                Array.headNonEmpty(settings.compilerPackages),
                held,
                Array.map(missed, ({ config }) => config),
            ),
    });
    const fresh = yield* Effect.forEach(solutions, ([, solution]) => Effect.result(resolved(workspaceRoot, held, solution)), { concurrency: 'unbounded' });
    const stored = Array.getSuccesses(Array.zipWith(unread, fresh, ({ config, digest }, program) => Result.map(program, (value) => [config, { ...value, digest }] as const)));
    yield* Effect.forEach(stored, ([config, value]) => Effect.sync(() => cache.set(config, value)));
    yield* Array.isReadonlyArrayNonEmpty(stored) ? Effect.sync(() => cache.writeToDisk()) : Effect.void;
    const standalone = Option.filter(yield* Effect.findFirst(['src', 'lib'], (at) => FileSystem.FileSystem.use((fs) => fs.exists(join(workspaceRoot, at)))), () => listed.has(MANIFEST));
    const lints = installed('eslint', join(workspaceRoot, MANIFEST))
        ? yield* linted(workspaceRoot, Array.headNonEmpty(settings.compilerPackages), exec, [...[...held].map((root) => ({ root, scope: root })), ...Option.toArray(Option.map(standalone, (scope) => ({ root: '.', scope })))], row)
        : new Map<string, TargetConfiguration>();
    const styles = Option.flatten(
        yield* Effect.transposeOption(
            Option.map(
                Option.filter(Option.fromUndefinedOr(settings.stylelintConfig), () => installed('stylelint', join(workspaceRoot, MANIFEST))),
                (config) => styled(workspaceRoot, Array.headNonEmpty(settings.compilerPackages), exec, row, config),
            ),
        ),
    );
    const rooted = getRootTsConfigFileName();
    const programs = new Map<string, Result.Result<Program, Cause.UnknownError>>([...Array.getSomes(rows.map(({ config, program }) => Option.map(program, (cached) => [config, Result.succeed(cached)] as const))), ...Array.zipWith(unread, fresh, ({ config }, program) => [config, program] as const)]);
    const chained = (root: string) => Option.match(Option.flatMap(Option.fromUndefinedOr(programs.get(posix.join(root, CONFIG))), Option.getSuccess), { onNone: () => [], onSome: ({ chain }) => chain.filter((at) => posix.relative(root, at).startsWith('..') && at !== rooted).map(tokened(root)) });
    const read = Effect.fnUntraced(function* (file: string) {
        const root = dirname(file);
        const manifest = yield* looked(manifests, root);
        const stated = manifest.nx?.targets ?? {};
        const typed = (program: Program) =>
            Option.map(
                Option.liftPredicate(program, ({ commands }) => Array.isReadonlyArrayNonEmpty(commands)),
                (checked) => projected(root, 'typescript', compiled(root, checked, settings.compilerPackages, `^${namedInput(root, context)}`, manifest.version !== undefined && installed('publint', join(workspaceRoot, file)), synced, exec), stated),
            );
        const loadable = Effect.sync(() => installed('cf', join(workspaceRoot, file)));
        const linting = Option.map(
            Option.filter(Option.fromUndefinedOr(lints.get(root)), () => anchors.has(file) || root === '.'),
            (eslint) => projected(root, 'eslint', { check: { eslint: { ...eslint, inputs: [...(eslint.inputs ?? []), ...chained(root)] } } }, stated),
        );
        const branch = yield* Match.value(file).pipe(
            Match.when(MANIFEST, () => workspace(workspaceRoot, manager, exec, settings.knipConfig, manifest, styles)),
            Match.when(
                (at) => basename(at) === PROJECT,
                () => Effect.succeedNone,
            ),
            Match.when(
                (at) => basename(at) === CONFIG,
                () => Effect.map(looked(programs, file), typed),
            ),
            Match.when(
                (at) => basename(at) === WORKER,
                () => Effect.when(worker(context, exec, stated, file), loadable),
            ),
            Match.orElse(() => Effect.succeed(tarball(manager, exec, settings.outputPath, manifest, root, tagged(context.nxJsonConfiguration)))),
        );
        return Option.reduceCompact([linting, branch], Option.none<ProjectConfiguration>(), merged);
    });
    const each = (file: string) => run(Effect.map(read(file), (project) => ({ projects: Record.getSomes({ [dirname(file)]: project }) })));
    return yield* Effect.promise(() => createNodesFromFiles(each, nested, settings, context));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [`**/{${CONFIG},${MANIFEST},${WORKER},${PROJECT}}`, nodes];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
