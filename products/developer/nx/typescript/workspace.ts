import { createRequire, findPackageJSON } from 'node:module';
import { dirname, join, posix, relative } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import type { loadConfig } from '@cloudflare/config';
import { type CreateNodesContext, normalizePath, type PackageManager, type ProjectConfiguration, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { type PackageJson, quoteShellArg } from '@nx/devkit/internal';
import { getLockFileName } from '@nx/js';
import { aggregated, manifested, namedInput, spawned, type Stated, tokened, type Verbs } from '@rasm/nx-distribution/inference';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Cause, Effect, FileSystem, Option, Record, Schema, UndefinedOr } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = 'package.json';

const WORKER = 'cloudflare.config.ts';

const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#/package.json'))).name;

const DUPLICATES = ['.jscpd.json', '.config/jscpd.json', '.config/.jscpd.json'];

const UNPACKAGED: NonNullable<TargetConfiguration['inputs']> = ['default', '^default', { externalDependencies: [] }];

const MANAGERS = {
    npm: { argv: ['npm', 'pack'], to: (at: string) => ['--pack-destination', at], workspace: { json: posix.join('{workspaceRoot}', MANIFEST), fields: ['workspaces'] }, catalogs: [], lock: ['install', 'dedupe'], prune: ['cache', 'verify'], stored: [] },
    pnpm: { argv: ['pnpm', 'pack'], to: (at: string) => ['--out', posix.join(at, '%s.tgz')], workspace: posix.join('{workspaceRoot}', 'pnpm-workspace.yaml'), catalogs: ['pnpm-workspace.yaml'], lock: ['dedupe'], prune: ['store', 'prune'], stored: ['store', 'path'] },
    yarn: { argv: ['yarn', 'pack'], to: (at: string) => ['--out', posix.join(at, '%s.tgz')], workspace: posix.join('{workspaceRoot}', '.yarnrc.yml'), catalogs: ['.yarnrc.yml'], lock: ['install', 'dedupe'], prune: ['cache', 'clean'], stored: [] },
    bun: { argv: ['bun', 'pm', 'pack'], to: (at: string) => ['--destination', at], workspace: { json: posix.join('{workspaceRoot}', MANIFEST), fields: ['workspaces'] }, catalogs: [], lock: ['install', 'dedupe'], prune: ['pm', 'cache', 'rm'], stored: [] },
} as const satisfies Record<
    PackageManager,
    { readonly argv: readonly string[]; readonly to: (at: string) => readonly string[]; readonly workspace: NonNullable<TargetConfiguration['inputs']>[number]; readonly catalogs: readonly string[]; readonly lock: readonly string[]; readonly prune: readonly string[]; readonly stored: readonly string[] }
>;

// --- [MODELS] --------------------------------------------------------------------------

class Manifest extends Schema.Opaque<Manifest, unknown>()(
    Schema.fromJsonString(
        Schema.Struct({
            version: Schema.optionalKey(Schema.String),
            os: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
            jscpd: Schema.optionalKey(Schema.Unknown),
        }),
    ).annotate({ description: "The version, os, and jscpd rows of a project's package.json, jscpd reading its configuration from the jscpd row" }),
) {}

const Exported = Schema.fromJsonString(Schema.Struct({ exports: Schema.Struct({ '.': Schema.Struct({ import: Schema.String }) }) })).annotate({ description: 'The import entry of the root export of @cloudflare/config, the package cf loads cloudflare.config.ts through' });

const Scanned = Schema.fromJsonString(Schema.Struct({ pattern: Schema.NullOr(Schema.String) })).annotate({ description: 'The pattern row of the merged configuration jscpd --debug prints, the glob of the files jscpd scans, null scanning every file' });

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

const installed = (name: string, from: string) => Option.isSome(Option.flatMap(Option.liftThrowable(findPackageJSON)(name, pathToFileURL(from)), Option.fromUndefinedOr));

const tarball = (manager: PackageManager, exec: string, outputPath: string | undefined, { manifest: { version, os }, stated }: { readonly manifest: Manifest; readonly stated: Stated }, root: string, released: readonly string[]) => {
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
        onDefined: () => Option.some(projected(root, 'npm', verbs, stated, { tags: [...released, ...systems], release: {} })),
    });
};

const workspace = Effect.fnUntraced(function* (workspaceRoot: string, manager: PackageManager, exec: string, knipConfig: string | undefined, { manifest: { jscpd }, stated }: { readonly manifest: Manifest; readonly stated: Stated }) {
    const { catalogs, prune } = MANAGERS[manager];
    const knip = ['knip', ...UndefinedOr.match(knipConfig, { onUndefined: () => [], onDefined: (at) => ['--config', at] })].map(quoteShellArg).join(' ');
    const configured = Option.orElse(yield* Effect.findFirst(DUPLICATES, (at) => FileSystem.FileSystem.use((fs) => fs.exists(join(workspaceRoot, at)))), () => Option.liftPredicate(MANIFEST, () => jscpd !== undefined));
    const duplicated = yield* Effect.transposeOption(
        Option.map(configured, (config) =>
            Effect.map(Effect.flatMap(spawned(['jscpd', '--debug', '.'], { cwd: workspaceRoot }), Schema.decodeEffect(Scanned)), ({ pattern }) => ({
                command: 'jscpd .',
                options: { cwd: '.' },
                cache: true,
                inputs: [posix.join('{workspaceRoot}', pattern ?? '**/*'), posix.join('{workspaceRoot}', config), { runtime: 'jscpd --version' }, { externalDependencies: [] }],
                outputs: [],
                metadata: { technologies: ['jscpd'], description: `Reports code duplicated across the workspace files ${config} selects with jscpd`, help: { command: 'jscpd --help', example: { args: ['--changed'] } } },
            })),
        ),
    );
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
                clean: {
                    [manager]: {
                        executor: `${distribution.name}:clean`,
                        configurations: { cache: { commands: [[manager, ...prune]] } },
                        inputs: [{ externalDependencies: [] }],
                        metadata: { description: `Under configuration cache, removes the packages no project of the store or cache of ${manager} references with ${manager} ${prune.join(' ')}`, help: { command: `${manager} ${prune.join(' ')} --help`, example: { args: [] } } },
                    },
                },
                check: {
                    ...Record.getSomes({ jscpd: duplicated }),
                    ...(installed('knip', join(workspaceRoot, MANIFEST)) && {
                        knip: {
                            command: knip,
                            options: { cwd: '.' },
                            cache: true,
                            inputs: [posix.join('{workspaceRoot}', '**/*.{js,mjs,cjs,jsx,ts,tsx,mts,cts,json,jsonc,json5,yml,yaml,toml}'), posix.join('{workspaceRoot}', getLockFileName(manager)), { externalDependencies: ['knip'] }],
                            outputs: [],
                            configurations: { ci: { args: '--reporter symbols --reporter github-actions' } },
                            metadata: { technologies: ['knip'], description: `Reports unused files, exports, and dependencies of every workspace package with ${knip}, adding GitHub annotations under the ci configuration`, help: { command: `${exec} knip --help`, example: { args: ['--production'] } } },
                        },
                    }),
                },
            },
            stated,
        ),
    );
});

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

// --- [EXPORTS] -------------------------------------------------------------------------

export { installed, loaded, MANAGERS, MANIFEST, projected, tarball, worker, WORKER, workspace };
