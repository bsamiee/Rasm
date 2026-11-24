import { createRequire, findPackageJSON } from 'node:module';
import { dirname, join, posix, relative } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import type { loadConfig } from '@cloudflare/config';
import { type CreateNodesContext, hashArray, normalizePath, type PackageManager, type ProjectConfiguration, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { getLockFileName, type PackageJson, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { aggregated, EXEC, manifested, namedInput, recalled, spawned, type Stated, stamped, tokened, type Verbs, within } from '@rasm/nx-distribution/inference';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Cause, Effect, FileSystem, Option, Record, Schema, UndefinedOr } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = 'package.json';
const WORKER = 'cloudflare.config.ts';
const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#/package.json'))).name;
const UNPACKAGED: NonNullable<TargetConfiguration['inputs']> = ['default', '^default', { externalDependencies: [] }];
const MANAGERS = {
    npm: { argv: ['npm', 'pack'], to: (at: string) => ['--pack-destination', at], catalogs: [], lock: ['install', 'dedupe'], prune: ['cache', 'verify'], stored: [] },
    pnpm: { argv: ['pnpm', 'pack'], to: (at: string) => ['--out', posix.join(at, '%s.tgz')], catalogs: ['pnpm-workspace.yaml'], lock: ['dedupe'], prune: ['store', 'prune'], stored: ['store', 'path'] },
    yarn: { argv: ['yarn', 'pack'], to: (at: string) => ['--out', posix.join(at, '%s.tgz')], catalogs: ['.yarnrc.yml'], lock: ['install', 'dedupe'], prune: ['cache', 'clean'], stored: [] },
    bun: { argv: ['bun', 'pm', 'pack'], to: (at: string) => ['--destination', at], catalogs: [], lock: ['install', 'dedupe'], prune: ['pm', 'cache', 'rm'], stored: [] },
} as const satisfies Record<PackageManager, { readonly argv: readonly string[]; readonly to: (at: string) => readonly string[]; readonly catalogs: readonly string[]; readonly lock: readonly string[]; readonly prune: readonly string[]; readonly stored: readonly string[] }>;

// --- [MODELS] --------------------------------------------------------------------------

const Manifest = Schema.fromJsonString(Schema.Struct({ version: Schema.optionalKey(Schema.String), os: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))), jscpd: Schema.optionalKey(Schema.Unknown) })).annotate({
    description: "The version, os, and jscpd rows of a project's package.json, jscpd reading its configuration from the jscpd row",
});
const Exported = Schema.fromJsonString(Schema.Struct({ exports: Schema.Struct({ '.': Schema.Struct({ import: Schema.String }) }) })).annotate({ description: 'The import entry of the root export of @cloudflare/config, the package cf loads cloudflare.config.ts through' });
const Scanned = Schema.fromJsonString(Schema.Struct({ pattern: Schema.NullOr(Schema.String), ignore: Schema.Array(Schema.String) })).annotate({
    description: 'The pattern and ignore rows of the merged configuration jscpd --debug prints: the glob of the files jscpd scans, null scanning every file, and the globs under the workspace it skips',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const loaded = Effect.fnUntraced(function* (workspaceRoot: string, root: string) {
    const fs = yield* FileSystem.FileSystem;
    const manifest = fs.readFileString(join(workspaceRoot, root, MANIFEST)).pipe(
        Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed('{}')),
        Effect.flatMap(Schema.decodeEffect(Manifest)),
    );
    return [root, yield* Effect.result(Effect.all({ manifest, stated: manifested(workspaceRoot, root) }, { concurrency: 'unbounded' }))] as const;
});

const projected = (root: string, technology: string, verbs: Verbs, stated: Stated, extra: Partial<ProjectConfiguration> = {}) => ({ root, metadata: { technologies: [technology] }, ...extra, targets: aggregated(verbs, stated, technology) }) satisfies ProjectConfiguration;

const installed = (name: string, from: string) => UndefinedOr.liftThrowable(findPackageJSON)(name, pathToFileURL(from)) !== undefined;

const tarball = (manager: PackageManager, outputPath: string | undefined, { manifest: { version, os }, stated }: { readonly manifest: typeof Manifest.Type; readonly stated: Stated }, root: string, released: readonly string[]) => {
    const { argv, to, catalogs } = MANAGERS[manager];
    const packed = `pack--${manager}`;
    const verbs: Verbs = {
        pack: {
            [manager]: {
                executor: `${PLUGIN}:pack`,
                options: UndefinedOr.match(outputPath, { onUndefined: () => ({ argv }), onDefined: (at) => ({ argv: [...argv, ...to(posix.relative(root, at))], outputPath: at }) }),
                dependsOn: ['build--*'],
                cache: true,
                inputs: ['default', '^{projectRoot}/package.json', Array.match(catalogs, { onEmpty: () => ({ json: posix.join('{workspaceRoot}', MANIFEST), fields: ['workspaces'] }), onNonEmpty: ([file]) => posix.join('{workspaceRoot}', file) }), { dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }],
                outputs: [tokened(root)(posix.join(outputPath ?? root, '*.tgz'))],
                metadata: { description: `Packs the package into one tarball named for it with ${argv.join(' ')}`, help: { command: `${argv.join(' ')} --help`, example: { args: ['--json'] } } },
            },
        },
        clean: { [manager]: { executor: `${distribution.name}:clean`, options: { targets: [packed] }, inputs: UNPACKAGED, metadata: { description: `Removes the outputs ${packed} declare`, help: { command: `${EXEC[manager]} nx run --help`, example: { options: { targets: [packed] } } } } } },
        attach: {
            gh: {
                executor: `${distribution.name}:attach`,
                dependsOn: [packed],
                inputs: [{ dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }],
                metadata: { description: `Uploads the files ${packed} wrote to the GitHub release of the tag nx release made for the project at the checked-out commit with gh release upload`, help: { command: 'gh release upload --help', example: { options: { dryRun: true } } } },
            },
        },
    };
    const systems = os.filter((system) => !system.startsWith('!')).map((system) => `os:${system}`);
    return UndefinedOr.match(version, {
        onUndefined: () => Option.map(Option.liftPredicate(systems, Array.isReadonlyArrayNonEmpty), (tags): ProjectConfiguration => ({ root, tags: [...tags] })),
        onDefined: () => Option.some(projected(root, 'npm', verbs, stated, { tags: [...released, ...systems], release: {} })),
    });
};

const workspace = Effect.fnUntraced(function* (workspaceRoot: string, manager: PackageManager, knipConfig: string | undefined, row: string, { manifest: { jscpd }, stated }: { readonly manifest: typeof Manifest.Type; readonly stated: Stated }) {
    const { catalogs, prune } = MANAGERS[manager];
    const lock = getLockFileName(manager);
    const knip = ['knip', ...UndefinedOr.match(knipConfig, { onUndefined: () => [], onDefined: (at) => ['--config', at] })].map(quoteShellArg).join(' ');
    const configured = Option.orElse(yield* Effect.findFirst(['.jscpd.json', '.config/jscpd.json', '.config/.jscpd.json'], (at) => FileSystem.FileSystem.use((fs) => fs.exists(join(workspaceRoot, at)))), () => Option.liftPredicate(MANIFEST, () => jscpd !== undefined));
    const duplicated = yield* Effect.transposeOption(
        Option.map(
            configured,
            Effect.fnUntraced(function* (config) {
                const [cache, key] = [new PluginCache<{ readonly sources: readonly string[]; readonly stamp: string } & typeof Scanned.Type>(join(workspaceDataDirectory, 'typescript-jscpd.hash')), hashArray([row, config])];
                const { pattern, ignore } = yield* Effect.flatMap(
                    recalled(cache, key, stamped(workspaceRoot)),
                    Option.match({
                        onSome: Effect.succeed,
                        onNone: Effect.fnUntraced(function* () {
                            const [scanned, stamp] = yield* Effect.all([Effect.flatMap(spawned(['jscpd', '--debug', '.'], { cwd: workspaceRoot }), Schema.decodeEffect(Scanned)), stamped(workspaceRoot)([config, lock])]);
                            const entry = { sources: [config, lock], stamp, ...scanned };
                            cache.set(key, entry);
                            cache.writeToDisk();
                            return entry;
                        }),
                    }),
                );
                return {
                    command: 'jscpd .',
                    inputs: [posix.join('{workspaceRoot}', pattern ?? '**/*'), ...ignore.map((glob) => `!${posix.join('{workspaceRoot}', glob)}`), posix.join('{workspaceRoot}', config), { runtime: 'jscpd --version' }, { externalDependencies: [] }],
                    metadata: { description: `Reports code duplicated across the workspace files ${config} selects with jscpd`, help: { command: 'jscpd --help', example: { args: ['--changed'] } } },
                };
            }),
        ),
    );
    const checks: Record<string, TargetConfiguration> = {
        ...Record.getSomes({ jscpd: duplicated }),
        ...(installed('knip', join(workspaceRoot, MANIFEST)) && {
            knip: {
                command: knip,
                inputs: [posix.join('{workspaceRoot}', '**/*.{js,mjs,cjs,jsx,ts,tsx,mts,cts,json,jsonc,json5,yml,yaml,toml}'), posix.join('{workspaceRoot}', lock), { externalDependencies: ['knip'] }],
                configurations: { ci: { args: '--reporter symbols --reporter github-actions' } },
                metadata: { description: `Reports unused files, exports, and dependencies of every workspace package with ${knip}, adding GitHub annotations under the ci configuration`, help: { command: `${EXEC[manager]} knip --help`, example: { args: ['--production'] } } },
            },
        }),
    };
    return projected(
        '.',
        manager,
        {
            upgrade: {
                [manager]: {
                    executor: `${PLUGIN}:upgrade`,
                    options: { catalogs },
                    inputs: [{ externalDependencies: [] }],
                    metadata: { description: `Moves every exact version of the catalogs and package manifests to the newest dist-tag head published since, then installs and deduplicates the lockfile with ${manager}`, help: { command: `${EXEC[manager]} nx run --help`, example: { options: { dryRun: true } } } },
                },
            },
            clean: {
                [manager]: {
                    executor: `${distribution.name}:clean`,
                    configurations: { cache: { commands: [[manager, ...prune]] } },
                    inputs: [{ externalDependencies: [] }],
                    metadata: { description: `Under configuration cache, removes the packages no project of the store or cache of ${manager} references with ${manager} ${prune.join(' ')}`, help: { command: `${manager} ${prune.join(' ')} --help`, example: { args: [] } } },
                },
            },
            check: Record.map(checks, (target, tool): TargetConfiguration => ({ options: { cwd: '.' }, cache: true, outputs: [], ...target, metadata: { technologies: [tool], ...target.metadata } })),
        },
        stated,
    );
});

const worker = Effect.fnUntraced(function* (context: CreateNodesContext, manager: PackageManager, stated: Stated, file: string, cache: PluginCache<{ readonly sources: readonly string[]; readonly stamp: string }>) {
    const fs = yield* FileSystem.FileSystem;
    const [root, at, lock] = [dirname(file), join(context.workspaceRoot, file), getLockFileName(manager)];
    const unresolved = (cause: unknown) => new Cause.UnknownError(cause, `${file} resolves no @cloudflare/config through cf`);
    const { sources } = yield* Effect.flatMap(
        recalled(cache, file, stamped(context.workspaceRoot)),
        Option.match({
            onSome: Effect.succeed,
            onNone: Effect.fnUntraced(function* () {
                const configured = yield* Effect.flatMap(Effect.try({ try: () => findPackageJSON('@cloudflare/config', pathToFileURL(createRequire(at).resolve('cf/package.json'))), catch: unresolved }), Effect.fromNullishOr);
                const { exports } = yield* Effect.flatMap(fs.readFileString(configured), Schema.decodeEffect(Exported));
                const cloudflare = yield* Effect.tryPromise({ try: (): Promise<{ readonly loadConfig: typeof loadConfig }> => import(pathToFileURL(join(dirname(configured), exports['.'].import)).href), catch: unresolved });
                const { dependencies } = yield* Effect.tryPromise({ try: () => cloudflare.loadConfig(at), catch: (cause) => new Cause.UnknownError(cause, `${file} does not load as a Cloudflare config`) });
                const read = [lock, ...Array.dedupe([file, ...Array.fromIterable(dependencies).map((dependency) => normalizePath(relative(context.workspaceRoot, dependency)))]).filter(within)];
                const entry = { sources: read, stamp: yield* stamped(context.workspaceRoot)(read) };
                cache.set(file, entry);
                cache.writeToDisk();
                return entry;
            }),
        }),
    );
    const typed = installed('@cloudflare/workers-types', at);
    return projected(
        root,
        'cloudflare',
        {
            codegen: {
                cf: {
                    command: typed ? 'cf workers types --include-runtime=false' : 'cf workers types',
                    options: { cwd: root },
                    dependsOn: ['^codegen--*'],
                    cache: true,
                    inputs: ['default', `^${namedInput(root, context)}`, ...sources.filter((dependency) => dependency !== lock && !dependency.startsWith(`${root}/`)).map((dependency) => posix.join('{workspaceRoot}', dependency)), { externalDependencies: ['cf'] }],
                    outputs: ['{projectRoot}/.cloudflare/types'],
                    metadata: {
                        description: typed ? `Generates the Worker binding types from ${WORKER} with cf workers types, runtime types coming from @cloudflare/workers-types` : `Generates the Worker binding and runtime types from ${WORKER} with cf workers types`,
                        help: { command: `${EXEC[manager]} cf workers types --help`, example: { args: ['--mode', 'production'] } },
                    },
                },
            },
            clean: { cf: { executor: `${distribution.name}:clean`, options: { targets: ['codegen--cf'] }, inputs: UNPACKAGED, metadata: { description: 'Removes the outputs codegen--cf declare', help: { command: `${EXEC[manager]} nx run --help`, example: { options: { targets: ['codegen--cf'] } } } } } },
        },
        stated,
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { installed, loaded, MANAGERS, MANIFEST, projected, tarball, worker, WORKER, workspace };
