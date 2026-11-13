/// <reference types="vitest/config" />

import { existsSync } from 'node:fs';
import { createRequire } from 'node:module';
import { basename, dirname, isAbsolute, join, matchesGlob, relative, resolve } from 'node:path';
import { env } from 'node:process';
import { pathToFileURL } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesContext, createNodesFromFiles, detectPackageManager, getPackageManagerCommand, normalizePath, type ProjectConfiguration, type TargetConfiguration } from '@nx/devkit';
import { getLockFileName, getNamedInputs, hashObject, hashWithWorkspaceContext, PluginCache, quoteShellArg, retryOnRequireEsmRace, workspaceDataDirectory } from '@nx/devkit/internal';
import { walkTsconfigExtendsChain } from '@nx/js/internal';
import { Array, Cause, Effect, Equal, FileSystem, Function, Option, Predicate, Record, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Resolved = Effect.Success<ReturnType<typeof read>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const EXTENSIONS = '{js,mjs,ts,cjs,mts,cts}';
const SUITE = `vitest.config.${EXTENSIONS}`;
const MODE = { TEST: 'true', VITEST: 'true', NODE_ENV: 'test' } as const;
const REPORTS: Record.ReadonlyRecord<string, string> = { html: '.vitest', blob: '.vitest/blob' };
const SHARED = '@rasm/nx-devkit';

// --- [MODELS] --------------------------------------------------------------------------

const Placed = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String) }).annotate({ description: 'outputPath: workspace folder a suite writes its reports under as <outputPath>/vitest/<project name>, absent leaves each reporter its own folder' });
const Reported = Schema.Struct({ outputFile: Schema.optionalKey(Schema.String), outputDir: Schema.optionalKey(Schema.String) }).annotate({ description: 'The file and folder options of a resolved [reporter, options] Vitest entry' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const refused = (subject: string, detail: string) => (cause?: unknown) => new Cause.UnknownError(cause, `${subject}: ${detail}`);

const run = <A, E>(effect: Effect.Effect<A, E, FileSystem.FileSystem>) => Effect.runPromise(Effect.provide(effect, layer));

const above = (folder: string): readonly string[] =>
    Array.unfold(
        Option.some(folder),
        Option.map((at: string) => [at, Option.liftPredicate(dirname(at), () => at !== '.')] as const),
    );

const extended = (workspaceRoot: string, root: string): readonly string[] => {
    const files: string[] = [];
    Option.toArray(
        Array.findFirst(
            above(root).map((at) => join(workspaceRoot, at, 'tsconfig.json')),
            existsSync,
        ),
    ).forEach((config) =>
        walkTsconfigExtendsChain(config, (at) => {
            files.push(at);
            return 'continue';
        }),
    );
    return files.map((at) => normalizePath(relative(workspaceRoot, at))).filter(kept);
};

const secluded = <A, E, R>(effect: Effect.Effect<A, E, R>) =>
    Effect.acquireUseRelease(
        Effect.sync(() => ({ ...env })),
        () =>
            Effect.andThen(
                Effect.sync(() => Object.assign(env, MODE)),
                effect,
            ),
        (before) =>
            Effect.sync(() => {
                Record.keys(MODE).forEach((name) => Reflect.deleteProperty(env, name));
                Object.assign(env, before);
            }),
    );

const directed = (to: string) => ['--coverage.reportsDirectory', join(to, 'coverage'), '--attachmentsDir', join(to, 'attachments'), '--outputFile.junit', join(to, 'junit.xml')];

const stamped = (workspaceRoot: string, sources: readonly string[]) => Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...sources]));

const kept = (at: string) => !(at.startsWith('..') || at.split('/').includes('node_modules'));

const loaded = <A>(file: string, own: string, specifier: string) => Effect.tryPromise({ try: (): Promise<A> => import(pathToFileURL(createRequire(own).resolve(specifier)).href), catch: refused(file, `resolves no ${specifier}`) });

const surveyed = Effect.fnUntraced(function* (workspaceRoot: string, file: string, flags: readonly string[]) {
    const fs = yield* FileSystem.FileSystem;
    const [folder, own] = [join(workspaceRoot, dirname(file)), join(workspaceRoot, file)];
    const local = (at: string) => normalizePath(relative(workspaceRoot, at));
    const [{ parseCLI, resolveConfig }, { configDefaults }] = yield* Effect.all([loaded<typeof import('vitest/node')>(file, own, 'vitest/node'), loaded<typeof import('vitest/config')>(file, own, 'vitest/config')]);
    const config = yield* Effect.tryPromise({
        try: () => retryOnRequireEsmRace(() => resolveConfig({ ...parseCLI(['vitest', 'run', ...flags]).options, config: own, root: folder, watch: false }, { logLevel: 'silent' })),
        catch: refused(file, 'does not resolve as a Vitest config'),
    });
    const { root, coverage, attachmentsDir, outputFile, reporters, forceRerunTriggers, resolvedProjects } = config.test;
    const declared = resolvedProjects.filter(({ inline, viteConfig }) => inline === true || viteConfig.configFile === own);
    const parents = new Map(declared.filter(({ hidden }) => hidden === true).map(({ viteConfig, projectConfig }) => [viteConfig, projectConfig.name] as const));
    const probes = yield* Effect.forEach(
        declared.filter(({ hidden }) => hidden !== true),
        Effect.fnUntraced(function* ({ viteConfig, projectConfig: { name, root: base, setupFiles, globalSetup, browser } }) {
            const manifests = above(local(base) || '.').map((at) => join(at, 'package.json'));
            const manifest = yield* Effect.mapError(
                Effect.flatMap(
                    Effect.findFirst(manifests, (at) => Effect.orElseSucceed(fs.exists(join(workspaceRoot, at)), Function.constFalse)),
                    Effect.fromOption,
                ),
                refused(file, `declares a project under ${base}, which no package holds`),
            );
            const owner = dirname(manifest);
            const setup = [...setupFiles, ...globalSetup].map(local).filter((at) => kept(at) && !at.startsWith(`${owner}/`));
            return {
                owner,
                project: parents.get(viteConfig) ?? name,
                manifests,
                inputs: [...setup, ...setup.flatMap((at) => extended(workspaceRoot, dirname(at)))],
                written: browser.enabled ? Array.getSomes([Option.fromUndefinedOr(browser.trace.tracesDir), Option.fromNullishOr(browser.screenshotDirectory)]) : [],
            };
        }),
    );
    const selectable = Array.dedupe(probes.map(({ project }) => project));
    const suites = Record.collect(
        Array.groupBy(probes, ({ owner }) => owner),
        (owner, group) => {
            const selected = Array.dedupe(group.map(({ project }) => project));
            return { root: owner, projects: selected.length === selectable.length ? [] : selected, inputs: Array.dedupe(group.flatMap(({ inputs }) => inputs)) };
        },
    );
    const keyed: Readonly<Record<string, string | undefined>> = Predicate.isString(outputFile) ? {} : (outputFile ?? {});
    const reports = reporters.flatMap((reporter) => {
        const [name, options] = Array.isArray(reporter) ? reporter : [reporter, {}];
        const decoded = Schema.decodeUnknownOption(Reported)(options);
        const built = Option.liftPredicate(name, Predicate.isString);
        const named = Option.flatMap(built, (key) => Option.fromUndefinedOr(keyed[key]));
        const stated = Array.getSomes([Option.firstSomeOf([Option.flatMap(decoded, ({ outputFile: direct }) => Option.fromUndefinedOr(direct)), Option.liftPredicate(outputFile, Predicate.isString), named]), Option.flatMap(decoded, ({ outputDir }) => Option.fromUndefinedOr(outputDir))]);
        return Array.isReadonlyArrayNonEmpty(stated) ? stated : Option.toArray(Option.flatMap(built, (key) => Record.get(REPORTS, key)));
    });
    const depended = Array.dedupe([own, ...config.configFileDependencies, ...declared.flatMap(({ viteConfig }) => viteConfig.configFileDependencies)].map(local)).filter(kept);
    return {
        suites,
        loaded: depended,
        triggers: Array.difference(forceRerunTriggers, configDefaults.forceRerunTriggers).map((glob) => (isAbsolute(glob) ? local(glob) : glob)),
        folders: Array.dedupe([coverage.reportsDirectory, attachmentsDir, ...reports, ...probes.flatMap((probe) => probe.written)].map((written) => local(resolve(root, written)))),
        sources: Array.dedupe([...depended, ...probes.flatMap((probe) => [...probe.manifests, ...probe.inputs])]),
    };
});

const read = Effect.fnUntraced(function* (workspaceRoot: string, file: string, flags: readonly string[], lock: string) {
    const [folder, own] = [join(workspaceRoot, dirname(file)), join(workspaceRoot, file)];
    const plain = yield* Effect.when(
        Effect.flatMap(loaded<typeof import('vite')>(file, own, 'vite'), ({ resolveConfig }) => Effect.tryPromise({ try: () => retryOnRequireEsmRace(() => resolveConfig({ configFile: own, root: folder, logLevel: 'silent' }, 'serve', 'test')), catch: refused(file, 'does not resolve as a Vite config') })),
        Effect.succeed(!matchesGlob(basename(file), SUITE)),
    );
    const report = yield* Option.match(
        Option.filter(plain, (resolved) => !Object.hasOwn(resolved, 'test')),
        {
            onNone: () => surveyed(workspaceRoot, file, flags),
            onSome: ({ configFileDependencies }): Effect.Effect<Effect.Success<ReturnType<typeof surveyed>>> => Effect.succeed({ suites: [], loaded: [], triggers: [], folders: [], sources: [file, ...configFileDependencies.map((at) => normalizePath(relative(workspaceRoot, at))).filter(kept)] }),
        },
    );
    const sources = [lock, ...report.sources];
    return { ...report, sources, stamp: yield* stamped(workspaceRoot, sources) };
});

const targets = (home: string, { loaded: configs, triggers, folders }: Resolved, { root, projects, inputs }: Resolved['suites'][number], chain: readonly string[], upstream: string, within: Option.Option<string>, exec: string): Record<string, TargetConfiguration> => {
    const held = Option.toArray(within);
    const flags = held.flatMap((at) => directed(relative(home, at)));
    const outside = Array.dedupe([...chain, ...configs, ...inputs]).filter((at) => root !== '.' && !at.startsWith(`${root}/`));
    return {
        'test--vitest': {
            command: [['vitest', 'run', ...projects.flatMap((name) => ['--project', name])].map(quoteShellArg).join(' '), ...Array.match(flags, { onEmpty: () => [], onNonEmpty: (stated) => ['{args}', stated.map(quoteShellArg).join(' ')] })].join(' '),
            options: { cwd: home },
            dependsOn: ['codegen--*', '^codegen'],
            cache: true,
            inputs: ['default', upstream, ...[...outside, ...triggers].map((at) => join('{workspaceRoot}', at)), { dependentTasksOutputFiles: '**/*', transitive: true }, { env: 'CI' }, { externalDependencies: ['vitest'] }],
            outputs: [...held, ...folders.filter((at) => !held.some((folder) => at.startsWith(`${folder}/`)))].map((at) => join('{workspaceRoot}', at)),
            metadata: { description: 'Runs every Vitest project of the suite with vitest run', help: { command: `${exec} vitest run --help`, example: { args: ['--project', '<name>'] } } },
        },
        'clean--vitest': {
            executor: `${SHARED}:clean`,
            options: { targets: ['test--vitest'], commands: [['vitest', '--clearCache']], cwd: home },
            metadata: { description: 'Removes the outputs test--vitest declares and every Vitest cache with vitest --clearCache', help: { command: `${exec} vitest --help`, example: {} } },
        },
        test: { dependsOn: ['test--*'] },
        clean: { dependsOn: ['clean--*'] },
    };
};

const inferred = Effect.fnUntraced(function* (cache: PluginCache<Resolved>, context: CreateNodesContext, within: Option.Option<string>, file: string) {
    const home = dirname(file);
    const manager = detectPackageManager(context.workspaceRoot);
    const flags = Option.toArray(within).flatMap((at) => directed(relative(home, at)));
    const held = yield* Effect.findFirst(Option.toArray(Option.fromUndefinedOr(cache.get(file))), ({ sources, stamp }) => Effect.map(stamped(context.workspaceRoot, sources), Equal.equals(stamp)));
    const resolved = yield* Option.match(held, { onSome: Effect.succeed, onNone: () => secluded(read(context.workspaceRoot, file, flags, getLockFileName(manager))) });
    cache.set(file, resolved);
    const projects = resolved.suites.map((suite): [string, ProjectConfiguration] => [
        suite.root,
        {
            root: suite.root,
            metadata: { technologies: ['vitest'] },
            targets: targets(home, resolved, suite, extended(context.workspaceRoot, suite.root), 'production' in getNamedInputs(suite.root, context) ? '^production' : '^default', within, getPackageManagerCommand(manager).exec),
        },
    ]);
    return { projects: Record.fromEntries(projects) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [
    `**/{vite,vitest}.config.${EXTENSIONS}`,
    (files, options, context) => {
        const configs = files.filter((file) => matchesGlob(basename(file), SUITE) || !files.some((other) => dirname(other) === dirname(file) && matchesGlob(basename(other), SUITE)));
        const within = Option.map(
            Option.flatMap(Schema.decodeOption(Placed)({ ...context.nxJsonConfiguration.pluginsConfig?.[SHARED], ...(Predicate.isObject(options) ? options : {}) }), ({ outputPath }) => Option.fromUndefinedOr(outputPath)),
            (at) => join(at, 'vitest', '{projectName}'),
        );
        const cache = new PluginCache<Resolved>(join(workspaceDataDirectory, `vitest-${hashObject({ options, within: Option.getOrNull(within) })}.hash`));
        return run(
            Effect.ensuring(
                Effect.tryPromise({
                    try: () => createNodesFromFiles((file) => run(inferred(cache, context, within, file)), configs, options, context),
                    catch: (cause) => (cause instanceof AggregateCreateNodesError ? cause : new AggregateCreateNodesError([[null, new Cause.UnknownError(cause, 'createNodes failed outside a file')]], [])),
                }),
                Effect.sync(() => cache.writeToDisk()),
            ),
        );
    },
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
