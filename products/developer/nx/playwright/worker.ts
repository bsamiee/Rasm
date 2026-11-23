import { createRequire, registerHooks } from 'node:module';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
import { parentPort, workerData } from 'node:worker_threads';
import { Array, Effect, Option, Predicate, Record, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROUTED: ReadonlySet<string> = new Set(['html', 'blob', 'json', 'junit', 'perfetto']);
const REPORTS: Readonly<Record<string, string>> = { html: 'playwright-report', blob: 'blob-report', perfetto: 'test-results', coverage: 'coverage' };

// --- [MODELS] --------------------------------------------------------------------------

const Task = Schema.Tuple([Schema.String, Schema.Boolean]).annotate({ description: 'The absolute path of a Playwright config and whether its run routes the built-in reporters reading PLAYWRIGHT_<NAME>_OUTPUT_DIR under outputPath' });
const Loader = Schema.Struct({
    configLoader: Schema.Struct({ loadConfigFromFile: Schema.declare((input: unknown): input is (file: string) => Promise<unknown> => Predicate.isFunction(input)) }),
    resolveReporterOutputPath: Schema.declare((input: unknown): input is (defaultValue: string, configDir: string, configValue: string | undefined) => string => Predicate.isFunction(input)),
    projectUtils: Schema.Struct({ collectFilesForProject: Schema.declare((input: unknown): input is (project: unknown, fsCache: Map<string, unknown>) => Promise<readonly string[]> => Predicate.isFunction(input)) }),
}).annotate({
    description:
        'The configLoader member of playwright/lib/common, the loader playwright test resolves its configuration with, the resolveReporterOutputPath member of playwright/lib/util, which resolves the folder a reporter states against the config folder and places the default folder of a built-in reporter beside the package.json nearest the config, and the projectUtils member of playwright/lib/runner, whose collectFilesForProject lists the test files a project runs through its testDir, testMatch, and testIgnore, walking each testDir once across the projects sharing its fsCache',
});
const Server = Schema.Struct({
    command: Schema.optional(Schema.String),
    reuseExistingServer: Schema.optional(Schema.Boolean),
    env: Schema.optional(Schema.Record(Schema.String, Schema.Unknown)),
    wait: Schema.optional(Schema.Struct({ stdout: Schema.optional(Schema.Unknown), stderr: Schema.optional(Schema.Unknown) })),
}).annotate({ description: 'A webServer row: the command Playwright starts, whether it reuses a server already listening, and the env and output wait that bind it to the process Playwright starts' });
const Full = Schema.Struct({
    configDir: Schema.String,
    globalSetups: Schema.Array(Schema.String),
    globalTeardowns: Schema.Array(Schema.String),
    webServers: Schema.Array(Server),
    config: Schema.Struct({ reporter: Schema.Array(Schema.Tuple([Schema.String, Schema.optionalKey(Schema.Unknown)])) }),
    projects: Schema.Array(Schema.Struct({ project: Schema.Struct({ name: Schema.String, outputDir: Schema.String, testDir: Schema.String, webServer: Schema.Array(Server) }) })),
}).annotate({ description: 'The FullConfigInternal loadConfigFromFile returns: config folder, global setup and teardown scripts, webServer rows, resolved reporters, and the name, output folder, test folder, and webServer rows of each project' });
const Listed = Schema.Struct({ projects: Schema.Array(Schema.Unknown) }).annotate({ description: 'The FullProjectInternal rows of a loaded config, each passed whole to collectFilesForProject' });
const Reported = Schema.Struct({ outputFile: Schema.optionalKey(Schema.String), outputDir: Schema.optionalKey(Schema.String), outputFolder: Schema.optionalKey(Schema.String) }).annotate({
    description: 'The file and folder options of a resolved [reporter, options] Playwright entry',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.fnUntraced(function* (entry: string, routed: boolean) {
    const playwright = createRequire(createRequire(entry).resolve('@playwright/test'));
    const { configLoader, resolveReporterOutputPath, projectUtils } = yield* Effect.flatMap(
        Effect.forEach(['playwright/lib/common', 'playwright/lib/util', 'playwright/lib/runner'], (at) => Effect.promise((): Promise<unknown> => import(pathToFileURL(playwright.resolve(at)).href))),
        (modules) => Schema.decodeUnknownEffect(Loader)(Object.assign({}, ...modules)),
    );
    const edges: (readonly [url: string, specifier: string, parent: string])[] = [];
    const full = yield* Effect.acquireUseRelease(
        Effect.sync(() =>
            registerHooks({
                resolve: (specifier, context, nextResolve) => {
                    const resolved = nextResolve(specifier, context);
                    edges.push(...(context.parentURL === undefined ? [] : [[resolved.url, specifier, context.parentURL] as const]));
                    return resolved;
                },
            }),
        ),
        () => Effect.promise(() => configLoader.loadConfigFromFile(entry)),
        (hooks) => Effect.sync(() => hooks.deregister()),
    );
    const [{ configDir, globalSetups, globalTeardowns, webServers, config, projects }, listed] = yield* Effect.all([Schema.decodeUnknownEffect(Full)(full), Schema.decodeUnknownEffect(Listed)(full)]);
    const walked = new Map<string, unknown>();
    const found = yield* Effect.forEach(listed.projects, (project) => Effect.promise(() => projectUtils.collectFilesForProject(project, walked)));
    const reports = config.reporter.map(([name, options]) => {
        const { outputFile, outputDir, outputFolder } = Option.getOrElse(Schema.decodeUnknownOption(Reported)(options), () => Reported.make({}));
        const [stated, routes] = [outputDir ?? outputFolder, routed && ROUTED.has(name)];
        const folder = Option.map(
            Option.orElse(Record.get(REPORTS, name), () => Option.fromUndefinedOr(stated)),
            (fallback) => resolveReporterOutputPath(fallback, configDir, stated),
        );
        return {
            name,
            routes,
            written: Option.toArray(
                Option.orElse(
                    Option.map(Option.fromUndefinedOr(outputFile), (at) => resolve(configDir, at)),
                    () => (routes ? Option.none() : folder),
                ),
            ),
        };
    });
    return {
        edges,
        scripts: [...globalSetups, ...globalTeardowns],
        routes: reports.filter(({ routes }) => routes).map(({ name }) => name),
        reported: reports.flatMap(({ written }) => written),
        commands: [...webServers, ...projects.flatMap(({ project }) => project.webServer)].flatMap(({ command, reuseExistingServer, env, wait }) => (reuseExistingServer === true && wait?.stdout === undefined && wait?.stderr === undefined && Record.isEmptyRecord(env ?? {}) ? Array.fromNullishOr(command) : [])),
        suites: Array.zipWith(projects, found, ({ project: { name, outputDir, testDir } }, files) => ({ name, outputDir, testDir, found: Array.isReadonlyArrayNonEmpty(files) })),
    };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

parentPort?.postMessage(await Effect.runPromise(Effect.flatMap(Schema.decodeUnknownEffect(Task)(workerData), ([entry, routed]) => read(entry, routed))));
