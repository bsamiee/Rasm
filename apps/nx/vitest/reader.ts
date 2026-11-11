/// <reference types="vitest/config" />
import { runtime, ToolError } from '@rasm/nx-devkit/process';
import { loaded, outermost, served } from '@rasm/nx-devkit/reply';
import { Array, Effect, FileSystem, Option, Path, Predicate, Record, Schema, String } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPORTS: Record.ReadonlyRecord<string, string> = { html: '.vitest', blob: '.vitest/blob', json: '.vitest/json', junit: '.vitest/junit' };

// --- [MODELS] --------------------------------------------------------------------------

const Job = Schema.TaggedUnion({
    Suite: { root: Schema.String, file: Schema.String, flags: Schema.Array(Schema.String) },
    Embedded: { root: Schema.String, file: Schema.String, flags: Schema.Array(Schema.String) },
}).annotate({ description: 'One config read under the test run environment and the flags its target passes: a Vitest config, or a Vite config that is a suite where it states test' });
const Report = Schema.Struct({ run: Schema.optionalKey(Schema.Struct({ folders: Schema.Array(Schema.String), files: Schema.Array(Schema.String), coverage: Schema.String })) }).annotate({
    description: 'The run of a config listing a test file: workspace-relative folders it writes, its test files relative to the config folder in name order, and its workspace-relative coverage folder',
});
const Reported = Schema.Struct({ outputFile: Schema.optionalKey(Schema.String), outputDir: Schema.optionalKey(Schema.String) }).annotate({ description: 'The file and folder options of a resolved [reporter, options] Vitest entry' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const collected = Effect.fnUntraced(function* ({ root: at, file, flags }: typeof Job.Type, workspaceRoot: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const folder = path.join(workspaceRoot, at);
    const [{ parseCLI, resolveConfig }, { glob }] = yield* Effect.all([loaded<typeof import('vitest/node')>(folder, 'vitest/node'), loaded<typeof import('tinyglobby')>(folder, 'vitest/package.json', 'tinyglobby')]);
    const config = yield* Effect.tryPromise({
        try: () => resolveConfig({ ...parseCLI(['vitest', 'run', ...flags]).options, config: path.join(workspaceRoot, file), root: folder, watch: false }, { logLevel: 'silent' }),
        catch: ToolError.of(file, 'does not resolve as a Vitest config'),
    });
    const { root, coverage, attachmentsDir, outputFile, reporters, resolvedProjects } = config.test;
    const projects = resolvedProjects.filter(({ hidden }) => hidden !== true).map(({ projectConfig }) => projectConfig);
    const matched = (cwd: string, patterns: readonly string[], exclude: readonly string[]) =>
        Array.match(patterns, { onEmpty: () => Effect.succeed(Array.empty<string>()), onNonEmpty: (include) => Effect.tryPromise({ try: () => glob(include, { cwd, ignore: [...exclude], dot: true, expandDirectories: false, absolute: true }), catch: ToolError.of(file, `lists no test file under ${cwd}`) }) });
    const probes = yield* Effect.forEach(
        projects,
        Effect.fnUntraced(function* ({ root: own, dir, include, exclude, includeSource = [], typecheck }) {
            const cwd = dir ?? own;
            const typed = typecheck.enabled && typecheck.only === true;
            const [tests, checks, sourced] = yield* Effect.all([matched(cwd, typed ? [] : include, exclude), matched(cwd, typecheck.enabled ? typecheck.include : [], typecheck.exclude), matched(cwd, typed ? [] : includeSource, exclude)]);
            const marked = yield* Effect.filter(sourced, (source) => Effect.map(Effect.option(fs.readFileString(source)), Option.exists(String.includes('import.meta.vitest'))));
            return { files: [...tests, ...checks, ...marked], sourced };
        }),
    );
    const specs = Array.dedupe(probes.flatMap((probe) => probe.files).map((spec) => path.relative(folder, spec))).toSorted();
    const stated = Option.liftPredicate(Predicate.isString);
    const keyed: Readonly<Record<string, string | undefined>> = Predicate.isString(outputFile) ? {} : (outputFile ?? {});
    const files = reporters.flatMap((reporter) => {
        const [name, options] = Array.isArray(reporter) ? reporter : [reporter, {}];
        const decoded = Schema.decodeUnknownOption(Reported)(options);
        const named = Option.flatMap(stated(name), (key) => Option.fromUndefinedOr(keyed[key]));
        const given = Array.getSomes([Option.firstSomeOf([Option.flatMap(decoded, ({ outputFile: own }) => Option.fromUndefinedOr(own)), stated(outputFile), named]), Option.flatMap(decoded, ({ outputDir }) => Option.fromUndefinedOr(outputDir))]);
        return Array.isReadonlyArrayNonEmpty(given) ? given : Option.toArray(Option.flatMap(stated(name), (key) => Record.get(REPORTS, key)));
    });
    const traced = projects.flatMap(({ browser }) => (browser.enabled ? Option.toArray(Option.fromUndefinedOr(browser.trace.tracesDir)) : []));
    const folders = outermost(
        path,
        workspaceRoot,
        [coverage.reportsDirectory, attachmentsDir, ...files, ...traced].map((written) => path.resolve(root, written)),
    );
    const run = { folders, files: specs, coverage: path.relative(workspaceRoot, path.resolve(root, coverage.reportsDirectory)) };
    return { report: Report.make(Array.isReadonlyArrayNonEmpty(specs) ? { run } : {}), sources: [config.configFileDependencies, ...resolvedProjects.map(({ viteConfig }) => viteConfig.configFileDependencies), ...probes.map((probe) => probe.sourced)].flat() };
});

const embedded = Effect.fnUntraced(function* (job: typeof Job.cases.Embedded.Type, workspaceRoot: string) {
    const path = yield* Path.Path;
    const folder = path.join(workspaceRoot, job.root);
    const { resolveConfig } = yield* loaded<typeof import('vite')>(folder, 'vite');
    const config = yield* Effect.tryPromise({ try: () => resolveConfig({ configFile: path.join(workspaceRoot, job.file), root: folder, logLevel: 'silent' }, 'serve', 'test'), catch: ToolError.of(job.file, 'does not resolve as a Vite config') });
    const { configFileDependencies: sources } = config;
    return Predicate.hasProperty(config, 'test') ? yield* collected(job, workspaceRoot) : { report: Report.make({}), sources };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

if (import.meta.main) {
    runtime.runPromise(served(Job, Report, (job, workspaceRoot) => Job.match(job, { Suite: (suite) => collected(suite, workspaceRoot), Embedded: (suite) => embedded(suite, workspaceRoot) })));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Job, Report };
