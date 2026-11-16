import { createRequire, findPackageJSON } from 'node:module';
import { basename, dirname, extname, join, relative, resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
import type { loadConfig } from '@cloudflare/config';
import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import { type CreateNodes, type CreateNodesContext, createNodesFromFiles, detectPackageManager, getPackageManagerCommand, hashArray, normalizePath, type PackageManager, type ProjectConfiguration, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { buildPackageJsonPatterns, buildPackageJsonWorkspacesMatcher, getFilesInDirectoryUsingContext, getLockFileName, getNamedInputs, hashFile, hashObject, hashWithWorkspaceContext, mergeTargetConfigurations, type PackageJson, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import type { API, ParsedCommandLine } from '@typescript/native/sync';
import { Array, Cause, Effect, Equal, FileSystem, Filter, Match, Option, Order, Predicate, Record, Result, Schema, type Scope, Stream, UndefinedOr } from 'effect';
import { ChildProcess } from 'effect/process';
import type { ESLint } from 'eslint';
import { getRootTsConfigFileName } from 'nx/src/plugins/js/utils/typescript';

// --- [TYPES] ---------------------------------------------------------------------------

type Program = Effect.Success<ReturnType<typeof resolved>>;
type Verbs = Readonly<Record<string, Readonly<Record<string, TargetConfiguration>>>>;
type Stated = Readonly<Record<string, Pick<TargetConfiguration, 'inputs'>>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const CONFIG = 'tsconfig.json';
const MANIFEST = 'package.json';
const WORKER = 'cloudflare.config.ts';
const PROJECT = 'project.json';
const LINTERS = ['eslint.config.js', 'eslint.config.mjs', 'eslint.config.cjs', 'eslint.config.ts', 'eslint.config.mts', 'eslint.config.cts'];
const DUPLICATES = ['.jscpd.json', '.config/jscpd.json', '.config/.jscpd.json'];
const PACKAGE = /^(?![./]|node:)(?<name>(?:@[^/]+\/)?[^/]+)/u;
const UNPACKAGED: NonNullable<TargetConfiguration['inputs']> = ['default', '^default', { externalDependencies: [] }];
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
    knipConfig: Schema.optionalKey(Schema.String),
}).annotate({
    description:
        'outputPath: workspace folder the package manager packs each tarball to, {projectName} naming the project, absent packs into the package folder. compilerPackages: npm packages every tsc target hashes as external dependencies, the first the native TypeScript package whose tsc runs and whose ./sync API reads each tsconfig.json. knipConfig: workspace file knip reads its configuration from through --config, absent leaving knip to find its own',
});
const Input = Schema.declare((input: unknown): input is NonNullable<TargetConfiguration['inputs']>[number] => Predicate.isString(input) || Predicate.isObject(input));
const Manifest = Schema.fromJsonString(
    Schema.Struct({
        version: Schema.optionalKey(Schema.String),
        os: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
        nx: Schema.optionalKey(Schema.Struct({ targets: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ inputs: Schema.optionalKey(Schema.mutable(Schema.Array(Input))) }))) })),
        jscpd: Schema.optionalKey(Schema.Unknown),
    }),
).annotate({ description: "The version, os, nx.targets, and jscpd rows of a project's package.json, Nx merging the inputs over the inferred targets after every plugin and jscpd reading its configuration from the jscpd row" });
const Literal = Schema.Struct({ text: Schema.String }).annotate({ description: 'A module specifier literal of a source file the native TypeScript API parses' });
const Exported = Schema.fromJsonString(Schema.Struct({ exports: Schema.Struct({ '.': Schema.Struct({ import: Schema.String }) }) })).annotate({ description: 'The import entry of the root export of @cloudflare/config, the package cf loads cloudflare.config.ts through' });
const Scanned = Schema.fromJsonString(Schema.Struct({ pattern: Schema.NullOr(Schema.String) })).annotate({ description: 'The pattern row of the merged configuration jscpd --debug prints, the glob of the files jscpd scans, null scanning every file' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const run = <A, E>(effect: Effect.Effect<A, E, NodeServices>) => Effect.runPromise(Effect.provide(effect, layer));

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

const tokened = (root: string) => (at: string) => (relative(root, at).startsWith('..') ? join('{workspaceRoot}', at) : join('{projectRoot}', relative(root, at)));

const installed = (name: string, from: string) => Option.isSome(Option.flatMap(Option.liftThrowable(findPackageJSON)(name, pathToFileURL(from)), Option.fromUndefinedOr));

const stamped = (workspaceRoot: string, sources: readonly string[]) => Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, Array.dedupe(sources.flatMap((at) => [at, join(dirname(at), '*')]))));

const kept = Effect.fnUntraced(function* (cache: PluginCache<Program>, workspaceRoot: string, lock: string, config: string) {
    const root = dirname(config);
    const [configured, listed] = yield* Effect.all([Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [`${root}/*.json`, `${root}/**/tsconfig*.json`, lock])), Effect.promise(() => getFilesInDirectoryUsingContext(workspaceRoot, root))], { concurrency: 'unbounded' });
    const key = hashArray([configured, ...listed, config]);
    const program = yield* Effect.findFirst(Array.fromNullishOr(cache.get(key)), ({ sources, stamp }) => Effect.map(stamped(workspaceRoot, sources), Equal.equals(stamp)));
    return { config, key, program };
});

const within = (at: string) => !(at.startsWith('..') || at.split('/').includes('node_modules'));

const referenced = ({ projectReferences = [] }: ParsedCommandLine) => projectReferences.map(({ path: at }) => (extname(at) === '.json' ? at : join(at, CONFIG)));

const connected = (workspaceRoot: string, compiler: string, from: string) =>
    Effect.flatMap(
        Effect.tryPromise({
            try: (): Promise<{ readonly API: typeof API }> => import(pathToFileURL(createRequire(resolve(workspaceRoot, from)).resolve(`${compiler}/sync`)).href),
            catch: (cause) => new Cause.UnknownError(cause, `${workspaceRoot} resolves no ${compiler}/sync`),
        }),
        (native) => Effect.acquireDisposable(Effect.sync(() => new native.API({ cwd: workspaceRoot }))),
    );

const solved = Effect.fnUntraced(function* (workspaceRoot: string, compiler: string, configs: Array.NonEmptyReadonlyArray<string>) {
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
    const parsed = (config: string) => {
        const file = resolve(workspaceRoot, config);
        const read = api.parseConfigFile(file);
        const [owned, external] = Array.partition(
            referenced(read),
            Filter.fromPredicate((at: string) => dirname(at) === dirname(file)),
        );
        return { entry: { file, parsed: read, chain: [file] }, owned, external: [...reached([...external, ...owned.flatMap(linked)], new Set([file, ...owned]))].filter((at) => !(at === file || owned.includes(at))) };
    };
    const reads = configs.map((config) => [config, Result.try({ try: () => parsed(config), catch: (cause) => new Cause.UnknownError(cause, `${config} does not parse as a tsconfig`) })] as const);
    const files = yield* Effect.filter(Array.dedupe(Array.getSuccesses(reads.map(([, read]) => read)).flatMap(({ entry, owned, external }) => [entry.file, ...owned, ...external])), (at) => fs.exists(at));
    const snapshot = yield* Effect.acquireDisposable(Effect.try({ try: () => api.createSnapshot({ openProjects: files }), catch: unopened }));
    const configured = (file: string) => Option.map(Option.fromNullishOr(snapshot.getConfiguredProject(file)), (project) => [file, { file, parsed: project.parsedCommandLine, chain: project.program.getConfigFileNames() }] as const);
    const opened = new Map(yield* Effect.try({ try: () => Array.getSomes(files.map(configured)), catch: unopened }));
    const members = (listed: readonly string[]) => Array.getSomes(listed.map((file) => Option.fromUndefinedOr(opened.get(file))));
    return reads.map(([config, read]) => [config, Result.map(read, ({ entry, owned, external }) => ({ config, entry: opened.get(entry.file) ?? entry, members: members(owned), external: members(external) }))] as const);
}, Effect.scoped);

const resolved = Effect.fnUntraced(function* (workspaceRoot: string, solution: Effect.Success<ReturnType<typeof solved>>[number][1]) {
    const { config, entry, members, external } = yield* Effect.fromResult(solution);
    const folder = join(workspaceRoot, dirname(config));
    const local = (at: string) => normalizePath(relative(workspaceRoot, at));
    const holding = [entry, ...members].filter(({ parsed }) => parsed.fileNames.length > 0);
    const checked = entry.parsed.fileNames.length > 0 || Array.isReadonlyArrayEmpty(holding) ? [entry] : holding;
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
    const sources = Array.dedupe([...[entry, ...holding].flatMap(({ chain, parsed }) => [...chain, ...parsed.fileNames]), ...external.flatMap(({ chain }) => chain)].map(local)).filter(within);
    return {
        commands: checked.map(({ file, parsed }) => {
            const named = basename(file) === CONFIG ? [] : [relative(folder, file)];
            return ['tsc', ...((parsed.projectReferences ?? []).length > 0 ? ['--build', ...named] : named.flatMap((at) => ['-p', at]))].map(quoteShellArg).join(' ');
        }),
        configs: checked.map(({ file }) => relative(folder, file)),
        outputs: Array.dedupe(outputs),
        chain: entry.chain.map(local).filter(within),
        sources,
        stamp: yield* stamped(workspaceRoot, sources),
    };
});

const compiled = (root: string, { commands, configs, outputs, sources }: Program, packages: readonly string[], upstream: string, linted: boolean, synced: boolean, exec: string): Verbs => {
    const metadata = (description: string, example: readonly string[]) => ({ description, help: { command: `${exec} tsc --help`, example: { args: [...example] } } });
    const token = tokened(root);
    const tsc = {
        executor: 'nx:run-commands',
        options: { commands, cwd: root },
        dependsOn: ['codegen--*', '^codegen', '^build--tsc'],
        cache: true,
        inputs: [...Array.dedupe([join(root, MANIFEST), ...sources].map(token)), upstream, { dependentTasksOutputFiles: '**/*', transitive: true }, { externalDependencies: [...packages] }],
        outputs: [],
        ...(synced && { syncGenerators: ['@nx/js:typescript-sync'] }),
    } satisfies TargetConfiguration;
    return Array.match(outputs, {
        onEmpty: () => ({ check: { tsc: { ...tsc, metadata: metadata('Type-checks every program of the project with tsc', ['--extendedDiagnostics']) } } }),
        onNonEmpty: (written) => {
            const build = { tsc: { ...tsc, outputs: written.map(token), metadata: metadata('Emits JavaScript and declarations with tsc, type-checking every program it builds', ['--listEmittedFiles']) } };
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
                tokened(root)(
                    join(
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
        onDefined: () => Option.some(projected(root, 'npm', verbs, nx?.targets ?? {}, { tags: ['release', ...systems], release: {} })),
    });
};

const workspace = Effect.fnUntraced(function* (workspaceRoot: string, manager: PackageManager, exec: string, { knipConfig }: typeof Options.Type, { nx, jscpd }: typeof Manifest.Type) {
    const { upgrade, example } = MANAGERS[manager];
    const command = upgrade.map(quoteShellArg).join(' ');
    const knip = ['knip', ...UndefinedOr.match(knipConfig, { onUndefined: () => [], onDefined: (at) => ['--config', at] })].map(quoteShellArg).join(' ');
    const configured = Option.orElse(yield* Effect.findFirst(DUPLICATES, (at) => FileSystem.FileSystem.use((fs) => fs.exists(join(workspaceRoot, at)))), () => Option.liftPredicate(MANIFEST, () => jscpd !== undefined));
    const debugged = Effect.flatMap(ChildProcess.make('jscpd', ['--debug', '.'], { cwd: workspaceRoot, stdin: 'ignore' }), (handle) =>
        Effect.all({ stdout: Stream.mkString(Stream.decodeText(handle.stdout)), stderr: Stream.mkString(Stream.decodeText(handle.stderr)), code: handle.exitCode }, { concurrency: 'unbounded' }),
    );
    const decoded = debugged.pipe(
        Effect.filterOrFail(
            ({ code }) => code === 0,
            ({ stderr, code }) => new Cause.UnknownError(code, `jscpd --debug . exits ${code} in ${workspaceRoot}: ${stderr.trim()}`),
        ),
        Effect.flatMap(({ stdout }) => Schema.decodeEffect(Scanned)(stdout)),
    );
    const scanned = yield* Effect.transposeOption(Option.as(configured, decoded));
    const duplicated = Option.zipWith(configured, scanned, (config, { pattern }) => ({
        command: 'jscpd .',
        options: { cwd: '.' },
        cache: true,
        inputs: [join('{workspaceRoot}', pattern ?? '**/*'), join('{workspaceRoot}', config), { runtime: 'jscpd --version' }, { externalDependencies: [] }],
        outputs: [],
        metadata: { technologies: ['jscpd'], description: `Reports code duplicated across the workspace files ${config} selects with jscpd`, help: { command: 'jscpd --help', example: { args: ['--changed'] } } },
    }));
    return Option.some(
        projected(
            '.',
            manager,
            {
                upgrade: {
                    [manager]: { command, inputs: [{ externalDependencies: [] }], metadata: { description: `Moves the dependency requirements of every workspace package and the lockfile to newer releases with ${command}`, help: { command: `${upgrade.slice(0, 2).join(' ')} --help`, example: { args: [...example] } } } },
                },
                check: {
                    ...Record.getSomes({ jscpd: duplicated }),
                    ...(installed('knip', join(workspaceRoot, MANIFEST)) && {
                        knip: {
                            command: knip,
                            options: { cwd: '.' },
                            cache: true,
                            inputs: [join('{workspaceRoot}', '**/*'), { externalDependencies: ['knip'] }],
                            outputs: [],
                            metadata: { technologies: ['knip'], description: `Reports unused files, exports, and dependencies of every workspace package with ${knip}`, help: { command: `${exec} knip --help`, example: { args: ['--production'] } } },
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

const merged = (prior: Option.Option<ProjectConfiguration>, next: ProjectConfiguration) => Option.some(Option.match(prior, { onNone: () => next, onSome: (held) => ({ ...held, ...next, targets: { ...held.targets, ...next.targets } }) }));

const looked = <A, E>(table: ReadonlyMap<string, Result.Result<A, E>>, at: string) => Effect.flatMap(Effect.fromNullishOr(table.get(at)), Effect.fromResult);

const imported = Effect.fnUntraced(function* (workspaceRoot: string, compiler: string, configs: Array.NonEmptyReadonlyArray<string>) {
    const api = yield* connected(workspaceRoot, compiler, Array.headNonEmpty(configs));
    const specified = (file: string) =>
        Effect.map(Effect.acquireDisposable(Effect.try({ try: () => api.createSourceFileFromFile(file), catch: (cause) => new Cause.UnknownError(cause, `${compiler}/sync parses no ${file}`) })), ({ sourceFile }) => {
            const texts = Array.getSomes(sourceFile.imports.map((node) => Schema.decodeUnknownOption(Literal)(node))).map(({ text }) => text);
            return { locals: texts.filter((text) => text.startsWith('.')).map((text) => join(dirname(file), text)), packages: Array.getSomes(texts.map((text) => Option.fromNullishOr(PACKAGE.exec(text)?.groups?.['name']))) };
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
    return new Map(yield* Effect.forEach(configs, (config) => Effect.map(reached([config], new Set(), new Set(['eslint'])), (held) => [config, held] as const)));
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
    const imports = yield* Array.match(Array.dedupe(Array.getSomes(governed.map(({ config }) => config))), { onEmpty: () => Effect.succeed(new Map<string, { readonly files: readonly string[]; readonly packages: readonly string[] }>()), onNonEmpty: (configs) => imported(workspaceRoot, compiler, configs) });
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
    const cache = new PluginCache<{ readonly targets: readonly TargetConfiguration[]; readonly read: readonly string[]; readonly digest: string }>(join(workspaceDataDirectory, `typescript-eslint-${hashed}.hash`));
    const lock = getLockFileName(detectPackageManager(workspaceRoot));
    const digested = (read: readonly string[]) => Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...read]));
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
            const stamp = yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [lock, ...Array.cartesianWith(ancestors, LINTERS, join)]));
            return { root, scope, owned, key: hashArray([stamp, root, ...owned]) };
        }),
        { concurrency: 'unbounded' },
    );
    const missed = yield* Effect.filter(keyed, ({ key }) =>
        Option.match(Option.fromNullishOr(cache.get(key)), {
            onNone: () => Effect.succeed(true),
            onSome: ({ read, digest }) => Effect.map(digested(read), (now) => now !== digest),
        }),
    );
    const fresh = yield* Array.match(missed, { onEmpty: () => Effect.succeed(new Map<string, { readonly read: readonly string[]; readonly targets: readonly TargetConfiguration[] }>()), onNonEmpty: (rows) => inspected(workspaceRoot, compiler, exec, rows) });
    yield* Effect.forEach(Array.getSomes(missed.map(({ root, key }) => Option.map(Option.fromUndefinedOr(fresh.get(root)), (held) => [key, held] as const))), ([key, { read, targets }]) => Effect.map(digested(read), (digest) => cache.set(key, { targets, read, digest })));
    yield* Array.isReadonlyArrayNonEmpty(missed) ? Effect.sync(() => cache.writeToDisk()) : Effect.void;
    return new Map(Array.getSomes(keyed.map(({ root, key }) => Option.map(Array.head(cache.get(key)?.targets ?? []), (target) => [root, target] as const))));
});

const nodes = Effect.fnUntraced(function* (files: readonly string[], options: typeof Options.Encoded | undefined, context: CreateNodesContext) {
    const { workspaceRoot } = context;
    const [settings, hashed] = yield* Effect.all([Schema.decodeEffect(Options)(options ?? {}), own]);
    const listed = new Set(files);
    const member = buildPackageJsonWorkspacesMatcher(buildPackageJsonPatterns(workspaceRoot, (file: string) => readJsonFile<PackageJson>(join(workspaceRoot, file))));
    const held = new Set(Array.dedupe(files.map(dirname)).filter((root) => root !== '.' && ((listed.has(join(root, MANIFEST)) && member(join(root, MANIFEST))) || listed.has(join(root, PROJECT)))));
    const nested = files.filter((file) => file === MANIFEST || held.has(dirname(file)));
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
    const standalone = Option.filter(yield* Effect.findFirst(['src', 'lib'], (at) => FileSystem.FileSystem.use((fs) => fs.exists(join(workspaceRoot, at)))), () => listed.has(MANIFEST));
    const lints = installed('eslint', join(workspaceRoot, MANIFEST))
        ? yield* linted(workspaceRoot, Array.headNonEmpty(settings.compilerPackages), exec, [...[...held].map((root) => ({ root, scope: root })), ...Option.toArray(Option.map(standalone, (scope) => ({ root: '.', scope })))], hashed)
        : new Map<string, TargetConfiguration>();
    const rooted = getRootTsConfigFileName();
    const programs = new Map<string, Result.Result<Program, Cause.UnknownError>>([...Array.getSomes(rows.map(({ config, program }) => Option.map(program, (cached) => [config, Result.succeed(cached)] as const))), ...Array.zipWith(unread, fresh, ({ config }, program) => [config, program] as const)]);
    const chained = (root: string) => Option.match(Option.flatMap(Option.fromUndefinedOr(programs.get(join(root, CONFIG))), Option.getSuccess), { onNone: () => [], onSome: ({ chain }) => chain.filter((at) => relative(root, at).startsWith('..') && at !== rooted).map(tokened(root)) });
    const read = Effect.fnUntraced(function* (file: string) {
        const root = dirname(file);
        const manifest = yield* looked(manifests, root);
        const stated = manifest.nx?.targets ?? {};
        const typed = (program: Program) => Option.some(projected(root, 'typescript', compiled(root, program, settings.compilerPackages, 'production' in getNamedInputs(root, context) ? '^production' : '^default', manifest.version !== undefined && installed('publint', join(workspaceRoot, file)), synced, exec), stated));
        const loadable = Effect.sync(() => installed('cf', join(workspaceRoot, file)));
        const linting = Option.map(
            Option.filter(Option.fromUndefinedOr(lints.get(root)), () => anchors.has(file) || root === '.'),
            (eslint) => projected(root, 'eslint', { check: { eslint: { ...eslint, inputs: [...(eslint.inputs ?? []), ...chained(root)] } } }, stated),
        );
        const branch = yield* Match.value(file).pipe(
            Match.when(MANIFEST, () => workspace(workspaceRoot, manager, exec, settings, manifest)),
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
            Match.orElse(() => Effect.succeed(tarball(manager, exec, settings, manifest, root))),
        );
        return Option.reduceCompact([linting, branch], Option.none<ProjectConfiguration>(), merged);
    });
    const each = (file: string) => run(Effect.map(read(file), (project) => ({ projects: Record.getSomes({ [dirname(file)]: project }) })));
    return yield* Effect.promise(() => createNodesFromFiles(each, nested, options, context));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = [`**/{${CONFIG},${MANIFEST},${WORKER},${PROJECT}}`, (files, options, context) => run(nodes(files, options, context))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
