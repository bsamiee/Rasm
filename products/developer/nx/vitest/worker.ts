import { createRequire, registerHooks } from 'node:module';
import { dirname, resolve } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { parentPort, workerData } from 'node:worker_threads';
import { retryOnRequireEsmRace } from '@nx/devkit/internal';
import { Array, Cause, Effect, Option, Predicate, Schema } from 'effect';
import type { configDefaults } from 'vitest/config';
import type { resolveConfig, Vite } from 'vitest/node';

// --- [MODELS] --------------------------------------------------------------------------

const Task = Schema.Struct({ file: Schema.String, entry: Schema.String, name: Schema.String, suite: Schema.Boolean, attachmentsDir: Schema.optionalKey(Schema.String) }).annotate({
    description: 'A Vite or Vitest config by its workspace and absolute path, the name of the plugin its root hook runs under, whether its file name marks a Vitest config, and the attachmentsDir the run writes under outputPath',
});
const Reported = Schema.Struct({ outputFile: Schema.optionalKey(Schema.String), outputDir: Schema.optionalKey(Schema.String) }).annotate({ description: 'The file and folder options of a resolved [reporter, options] Vitest entry' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const loaded = <A>(file: string, entry: string, specifier: string) => Effect.tryPromise({ try: (): Promise<A> => import(pathToFileURL(createRequire(entry).resolve(specifier)).href), catch: (cause) => new Cause.UnknownError(cause, `${file} resolves no ${specifier}`) });

const surveyed = Effect.fnUntraced(function* ({ file, entry, name, attachmentsDir }: typeof Task.Type) {
    const folder = dirname(entry);
    const [runner, defaults] = yield* Effect.all([loaded<{ readonly resolveConfig: typeof resolveConfig }>(file, entry, 'vitest/node'), loaded<{ readonly configDefaults: typeof configDefaults }>(file, entry, 'vitest/config')]);
    const config = yield* Effect.tryPromise({
        try: () =>
            retryOnRequireEsmRace(() =>
                runner.resolveConfig(
                    { config: entry, watch: false, ...(attachmentsDir !== undefined && { attachmentsDir }) },
                    { logLevel: 'silent', plugins: [{ name, enforce: 'pre', config: { order: 'pre', handler: ({ root: base, test }) => ({ root: resolve(folder, base ?? '.'), ...(test?.root !== undefined && { test: { root: resolve(folder, test.root) } }) }) } }] },
                ),
            ),
        catch: (cause) => new Cause.UnknownError(cause, `${file} does not load as a Vitest config`),
    });
    const { root, coverage, attachmentsDir: attached, outputFile, reporters, forceRerunTriggers, projects: listed, resolvedProjects } = config.test;
    const declared = resolvedProjects.filter(({ inline, viteConfig: { configFile } }) => inline === true || configFile === undefined || configFile === entry);
    const parents = new Map(declared.filter(({ hidden }) => hidden === true).map(({ viteConfig, projectConfig }) => [viteConfig, projectConfig.name] as const));
    const keyed: Readonly<Record<string, string | undefined>> = Predicate.isObject(outputFile) ? outputFile : {};
    const entries = reporters.map((reporter) => (Array.isArray(reporter) ? reporter : [reporter, {}])).map(([named, options]) => [named, Schema.decodeUnknownOption(Reported)(options)] as const);
    return {
        _tag: 'Suite' as const,
        root,
        reportsDirectory: coverage.reportsDirectory,
        covered: coverage.enabled,
        providers: coverage.provider === 'custom' ? Array.fromNullishOr(coverage.customProviderModule) : [`@vitest/coverage-${coverage.provider}`],
        attachmentsDir: attached,
        reports: entries.flatMap(([named, decoded]) =>
            Array.getSomes([
                Option.firstSomeOf([Option.flatMap(decoded, ({ outputFile: direct }) => Option.fromUndefinedOr(direct)), Option.liftPredicate(outputFile, Predicate.isString), Option.flatMap(Option.liftPredicate(named, Predicate.isString), (key) => Option.fromUndefinedOr(keyed[key]))]),
                Option.flatMap(decoded, ({ outputDir }) => Option.fromUndefinedOr(outputDir)),
            ]),
        ),
        triggers: Array.difference(forceRerunTriggers, defaults.configDefaults.forceRerunTriggers),
        listed: Option.isSome(Option.fromNullishOr(listed)),
        dependencies: Array.dedupe([entry, ...config.configFileDependencies, ...declared.flatMap(({ viteConfig }) => viteConfig.configFileDependencies)]),
        projects: declared
            .filter(({ hidden }) => hidden !== true)
            .map(({ viteConfig, projectConfig: { name: stated, root: base, setupFiles, globalSetup, browser } }) => ({
                project: parents.get(viteConfig) ?? stated,
                base,
                setup: [...setupFiles, ...Array.ensure(globalSetup)],
                written: browser.enabled ? Array.getSomes([Option.fromUndefinedOr(browser.trace.tracesDir), Option.fromNullishOr(browser.screenshotDirectory)]) : [],
                providers: browser.enabled ? Array.fromNullishOr(browser.provider?.name) : [],
            })),
    };
});

const read = Effect.fnUntraced(function* (task: typeof Task.Type) {
    const resolved = new Set<string>();
    yield* Effect.acquireRelease(
        Effect.sync(() =>
            registerHooks({
                resolve: (specifier, context, nextResolve) => {
                    const module = nextResolve(specifier, context);
                    resolved.add(module.url);
                    return module;
                },
            }),
        ),
        (hooks) => Effect.sync(() => hooks.deregister()),
    );
    const plain = yield* Effect.when(
        Effect.flatMap(loaded<typeof Vite>(task.file, task.entry, 'vite'), (vite) =>
            Effect.tryPromise({ try: () => retryOnRequireEsmRace(() => vite.resolveConfig({ configFile: task.entry, root: dirname(task.entry), logLevel: 'silent' }, 'serve', 'test')), catch: (cause) => new Cause.UnknownError(cause, `${task.file} does not load for vite serve`) }),
        ),
        Effect.succeed(!task.suite),
    );
    const report = yield* Option.match(
        Option.filter(plain, (config) => !Object.hasOwn(config, 'test')),
        {
            onSome: ({ configFileDependencies }) => Effect.succeed({ _tag: 'Plain' as const, dependencies: configFileDependencies }),
            onNone: () => surveyed(task),
        },
    );
    return {
        ...report,
        dependencies: Array.dedupe([
            ...report.dependencies,
            ...Array.getSomes(
                Array.map(
                    Array.fromIterable(resolved),
                    Option.liftThrowable((url: string) => fileURLToPath(url)),
                ),
            ),
        ]),
    };
}, Effect.scoped);

// --- [COMPOSITION] ---------------------------------------------------------------------

parentPort?.postMessage(await Effect.runPromise(Effect.flatMap(Schema.decodeUnknownEffect(Task)(workerData), read)));
