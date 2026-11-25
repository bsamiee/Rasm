import { createRequire, findPackageJSON } from 'node:module';
import { dirname, join, posix, relative } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import type { loadConfig } from '@cloudflare/config';
import { hashArray, normalizePath, type PackageManager, type ProjectConfiguration, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { getCatalogManager, getLockFileName, hashFile, type PackageJson, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { type Anchor, aggregated, EXEC, EXECUTORS, manifested, type Pass, recalled, spawned, type Stated, stamped, tokened, type Verbs, within } from '@rasm/nx';
import { Array, Cause, Effect, FileSystem, Option, Record, Schema, UndefinedOr } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = 'package.json';
const WORKER = 'cloudflare.config.ts';
const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#/package.json'))).name;
const UNPACKAGED: NonNullable<TargetConfiguration['inputs']> = ['default', '^default', { externalDependencies: [] }];
const MANAGERS = {
    npm: { argv: ['npm', 'pack'], to: (at: string) => ['--pack-destination', at], lock: ['install', 'dedupe'], prune: ['cache', 'verify'], stored: [] },
    pnpm: { argv: ['pnpm', 'pack'], to: (at: string) => ['--out', posix.join(at, '%s.tgz')], lock: ['dedupe'], prune: ['store', 'prune'], stored: ['store', 'path'] },
    yarn: { argv: ['yarn', 'pack'], to: (at: string) => ['--out', posix.join(at, '%s.tgz')], lock: ['install', 'dedupe'], prune: ['cache', 'clean'], stored: [] },
    bun: { argv: ['bun', 'pm', 'pack'], to: (at: string) => ['--destination', at], lock: ['install', 'dedupe'], prune: ['pm', 'cache', 'rm'], stored: [] },
} as const satisfies Record<PackageManager, { readonly argv: readonly string[]; readonly to: (at: string) => readonly string[]; readonly lock: readonly string[]; readonly prune: readonly string[]; readonly stored: readonly string[] }>;

// --- [MODELS] --------------------------------------------------------------------------

const Manifest = Schema.fromJsonString(Schema.Struct({ version: Schema.optionalKey(Schema.String), os: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))), jscpd: Schema.optionalKey(Schema.Unknown) })).annotate({
    description: "The version, os, and jscpd rows of a project's package.json, jscpd reading its configuration from the jscpd row",
});
const Exported = Schema.fromJsonString(Schema.Struct({ exports: Schema.Struct({ '.': Schema.Struct({ import: Schema.String }) }) })).annotate({ description: 'The import entry of the root export of @cloudflare/config, the package cf loads cloudflare.config.ts through' });
const Plugged = Schema.Struct({ Plugins: Schema.Record(Schema.String, Schema.Struct({ config: Schema.optionalKey(Schema.Unknown), isRootOnly: Schema.optionalKey(Schema.Boolean) })) }).annotate({
    description: 'The plugin map knip/dist/plugins.js exports: the config globs of each plugin, relative to every workspace or to the root alone under isRootOnly, a function standing for globs knip computes from a manifest',
});
const Globs = Schema.Array(Schema.String);
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

const projected = (root: string, technology: string, verbs: Verbs, stated: Stated, anchor: Anchor, extra: Partial<ProjectConfiguration> = {}) => ({ root, metadata: { technologies: [technology] }, ...extra, targets: aggregated(verbs, stated, technology, anchor) }) satisfies ProjectConfiguration;

const installed = (name: string, from: string) => UndefinedOr.liftThrowable(findPackageJSON)(name, pathToFileURL(from)) !== undefined;

const tarball = (manager: PackageManager, outputPath: string | undefined, { manifest: { version, os }, stated }: { readonly manifest: typeof Manifest.Type; readonly stated: Stated }, root: string, released: readonly string[], anchor: Anchor) => {
    const { argv, to } = MANAGERS[manager];
    const catalogs = getCatalogManager(anchor.workspaceRoot)?.getCatalogDefinitionFilePaths() ?? [];
    const packed = `pack--${manager}`;
    const verbs: Verbs = {
        pack: {
            [manager]: {
                executor: `${PLUGIN}:pack`,
                options: UndefinedOr.match(outputPath, { onUndefined: () => ({ argv }), onDefined: (at) => ({ argv: [...argv, ...to(posix.relative(root, at))], outputPath: at }) }),
                dependsOn: ['build--*'],
                cache: true,
                inputs: [
                    'default',
                    '^{projectRoot}/package.json',
                    ...Array.match(
                        catalogs.filter((file) => file !== MANIFEST),
                        { onEmpty: () => [{ json: posix.join('{workspaceRoot}', MANIFEST), fields: ['workspaces'] }], onNonEmpty: (files) => files.map((file) => posix.join('{workspaceRoot}', file)) },
                    ),
                    { dependentTasksOutputFiles: '**/*' },
                    { externalDependencies: [] },
                ],
                outputs: [tokened(root)(posix.join(outputPath ?? root, '*.tgz'))],
                metadata: { description: `Packs the package into one tarball named for it with ${argv.join(' ')}`, help: { command: `${argv.join(' ')} --help`, example: { args: ['--json'] } } },
            },
        },
        clean: { [manager]: { executor: EXECUTORS.clean, options: { targets: [packed] }, inputs: UNPACKAGED, metadata: { description: `Removes the outputs ${packed} declare`, help: { command: `${EXEC[manager]} nx run --help`, example: { options: { targets: [packed] } } } } } },
        attach: {
            gh: {
                executor: EXECUTORS.attach,
                dependsOn: [packed],
                inputs: [{ dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }],
                metadata: {
                    description: `Uploads the files ${packed} wrote to the GitHub release of the tag nx release made for the project at the checked-out commit with gh release upload, creating that release with gh release create where none exists`,
                    help: { command: 'gh release upload --help', example: { options: { dryRun: true } } },
                },
            },
        },
    };
    const systems = os.filter((system) => !system.startsWith('!')).map((system) => `os:${system}`);
    return UndefinedOr.match(version, {
        onUndefined: () => Option.map(Option.liftPredicate(systems, Array.isReadonlyArrayNonEmpty), (tags): ProjectConfiguration => ({ root, tags: [...tags] })),
        onDefined: () => Option.some(projected(root, 'npm', verbs, stated, anchor, { tags: [...released, ...systems], release: {} })),
    });
};

const plugins = Effect.fnUntraced(function* (workspaceRoot: string, row: string, manifest: string) {
    const cache = new PluginCache<{ readonly sources: readonly string[]; readonly configs: readonly string[] } & { readonly stamp: string }>(join(workspaceDataDirectory, 'typescript-knip.hash'));
    const read = Effect.tryPromise({ try: (): Promise<unknown> => import(new URL('dist/plugins.js', pathToFileURL(manifest)).href), catch: (cause) => new Cause.UnknownError(cause, `${manifest} holds no knip plugin map at dist/plugins.js`) }).pipe(
        Effect.flatMap(Schema.decodeUnknownEffect(Plugged)),
        Effect.map(({ Plugins }) => ({ sources: [], configs: Array.dedupe(Object.values(Plugins).flatMap(({ config, isRootOnly = false }) => Array.cartesianWith([isRootOnly ? '{workspaceRoot}' : '{workspaceRoot}/**'], Schema.is(Globs)(config) ? config : [], posix.join))) })),
    );
    return (yield* recalled(cache, hashArray([row, manifest, hashFile(manifest)]), stamped(workspaceRoot), read, () => cache.writeToDisk())).configs;
});

const workspace = Effect.fnUntraced(function* (anchor: Anchor, manager: PackageManager, knipConfig: string | undefined, row: string, { manifest: { jscpd }, stated }: { readonly manifest: typeof Manifest.Type; readonly stated: Stated }) {
    const { workspaceRoot } = anchor;
    const { prune } = MANAGERS[manager];
    const lock = getLockFileName(manager);
    const knip = ['knip', ...(UndefinedOr.map(knipConfig, (at) => ['--config', at]) ?? [])].map(quoteShellArg).join(' ');
    const configured = Option.orElse(yield* Effect.findFirst(['.jscpd.json', '.config/jscpd.json', '.config/.jscpd.json'], (at) => FileSystem.FileSystem.use((fs) => fs.exists(join(workspaceRoot, at)))), () => Option.liftPredicate(MANIFEST, () => jscpd !== undefined));
    const current = yield* Effect.gen(function* () {
        const config = yield* Effect.fromOption(configured);
        const cache = new PluginCache<{ readonly sources: readonly string[] } & typeof Scanned.Type & { readonly stamp: string }>(join(workspaceDataDirectory, 'typescript-jscpd.hash'));
        const scanned = Effect.map(spawned(['jscpd', '--debug', '.'], { cwd: workspaceRoot }).pipe(Effect.flatMap(Schema.decodeEffect(Scanned))), (read) => ({ sources: [config, ...(installed('jscpd', join(workspaceRoot, MANIFEST)) ? [lock] : [])], ...read }));
        return yield* recalled(cache, hashArray([row, config]), stamped(workspaceRoot), scanned, () => cache.writeToDisk());
    }).pipe(Effect.catchNoSuchElement);
    const plugged = yield* Effect.transposeOption(Option.map(Option.fromUndefinedOr(UndefinedOr.liftThrowable(findPackageJSON)('knip', pathToFileURL(join(workspaceRoot, MANIFEST)))), (manifest) => plugins(workspaceRoot, row, manifest)));
    const checks = Record.getSomes<string, TargetConfiguration>({
        jscpd: Option.zipWith(configured, current, (config, { pattern, ignore }) => ({
            command: 'jscpd .',
            inputs: [posix.join('{workspaceRoot}', pattern ?? '**/*'), ...ignore.map((glob) => `!${posix.join('{workspaceRoot}', glob)}`), posix.join('{workspaceRoot}', config), { runtime: 'jscpd --version' }, { externalDependencies: [] }],
            metadata: { description: `Reports code duplicated across the workspace files ${config} selects with jscpd`, help: { command: 'jscpd --help', example: { args: ['--changed'] } } },
        })),
        knip: Option.map(plugged, (configs) => ({
            command: knip,
            inputs: [posix.join('{workspaceRoot}', '**/*.{js,mjs,cjs,jsx,ts,tsx,mts,cts,json,jsonc,json5}'), ...configs, posix.join('{workspaceRoot}', lock), { externalDependencies: ['knip'] }],
            configurations: { ci: { args: '--reporter symbols --reporter github-actions' } },
            metadata: { description: `Reports unused files, exports, and dependencies of every workspace package with ${knip}, adding GitHub annotations under the ci configuration`, help: { command: `${EXEC[manager]} knip --help`, example: { args: ['--production'] } } },
        })),
    });
    return projected(
        '.',
        manager,
        {
            upgrade: {
                [manager]: {
                    executor: `${PLUGIN}:upgrade`,
                    options: { catalogs: getCatalogManager(workspaceRoot)?.getCatalogDefinitionFilePaths() ?? [] },
                    inputs: [{ externalDependencies: [] }],
                    metadata: { description: `Moves every exact version of the catalogs and package manifests to the newest dist-tag head published since, then installs and deduplicates the lockfile with ${manager}`, help: { command: `${EXEC[manager]} nx run --help`, example: { options: { dryRun: true } } } },
                },
            },
            clean: {
                [manager]: {
                    executor: EXECUTORS.clean,
                    configurations: { cache: { commands: [[manager, ...prune]] } },
                    inputs: [{ externalDependencies: [] }],
                    metadata: { description: `Under configuration cache, removes the packages no project of the store or cache of ${manager} references with ${manager} ${prune.join(' ')}`, help: { command: `${manager} ${prune.join(' ')} --help`, example: { args: [] } } },
                },
            },
            check: Record.map(checks, (target, tool): TargetConfiguration => ({ options: { cwd: '.' }, cache: true, outputs: [], ...target, metadata: { technologies: [tool], ...target.metadata } })),
        },
        stated,
        anchor,
    );
});

const worker = Effect.fnUntraced(function* (anchor: Anchor, manager: PackageManager, stated: Stated, file: string, recall: Pass<{ readonly sources: readonly string[] }>['recall']) {
    const fs = yield* FileSystem.FileSystem;
    const [root, at] = [dirname(file), join(anchor.workspaceRoot, file)];
    const unresolved = (cause: unknown) => new Cause.UnknownError(cause, `${file} resolves no @cloudflare/config through cf`);
    const read = Effect.gen(function* () {
        const configured = yield* Effect.try({ try: () => findPackageJSON('@cloudflare/config', pathToFileURL(createRequire(at).resolve('cf/package.json'))), catch: unresolved }).pipe(Effect.flatMap(Effect.fromNullishOr));
        const { exports } = yield* fs.readFileString(configured).pipe(Effect.flatMap(Schema.decodeEffect(Exported)));
        const cloudflare = yield* Effect.tryPromise({ try: (): Promise<{ readonly loadConfig: typeof loadConfig }> => import(pathToFileURL(join(dirname(configured), exports['.'].import)).href), catch: unresolved });
        const { dependencies } = yield* Effect.tryPromise({ try: () => cloudflare.loadConfig(at), catch: (cause) => new Cause.UnknownError(cause, `${file} does not load as a Cloudflare config`) });
        return { sources: Array.dedupe([file, ...dependencies.values().map((dependency) => normalizePath(relative(anchor.workspaceRoot, dependency)))]).filter(within) };
    });
    const { sources } = yield* recall(file, read);
    const typed = installed('@cloudflare/workers-types', at);
    return projected(
        root,
        'cloudflare',
        {
            codegen: {
                cf: {
                    command: typed ? 'cf workers types --include-runtime=false' : 'cf workers types',
                    options: { cwd: root },
                    cache: true,
                    inputs: [...sources.map(tokened(root)), { externalDependencies: ['cf'] }],
                    outputs: ['{projectRoot}/.cloudflare/types'],
                    metadata: {
                        description: typed ? `Generates the Worker binding types from ${WORKER} with cf workers types, runtime types coming from @cloudflare/workers-types` : `Generates the Worker binding and runtime types from ${WORKER} with cf workers types`,
                        help: { command: `${EXEC[manager]} cf workers types --help`, example: { args: ['--mode', 'production'] } },
                    },
                },
            },
            clean: { cf: { executor: EXECUTORS.clean, options: { targets: ['codegen--cf'] }, inputs: UNPACKAGED, metadata: { description: 'Removes the outputs codegen--cf declare', help: { command: `${EXEC[manager]} nx run --help`, example: { options: { targets: ['codegen--cf'] } } } } } },
        },
        stated,
        anchor,
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { installed, loaded, MANAGERS, MANIFEST, projected, tarball, worker, WORKER, workspace };
