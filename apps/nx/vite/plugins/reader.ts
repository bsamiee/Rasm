/// <reference types="vitest/config" />
import { createRequire } from 'node:module';
import { pathToFileURL } from 'node:url';
import { runtime, ToolError } from '@rasm/nx-devkit/process';
import { served } from '@rasm/nx-devkit/reply';
import { Array, Effect, FileSystem, Option, Path, Predicate, Record, Schema, Stream, String } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPORTS: Record.ReadonlyRecord<string, string> = { html: '.vitest', blob: '.vitest/blob', json: '.vitest/json', junit: '.vitest/junit' };

// --- [MODELS] --------------------------------------------------------------------------

const Job = Schema.TaggedUnion({
    Bundle: { root: Schema.String, file: Schema.String, outDir: Schema.optionalKey(Schema.String) },
    Suite: { root: Schema.String, file: Schema.String, flags: Schema.Array(Schema.String) },
    Embedded: { root: Schema.String, file: Schema.String, flags: Schema.Array(Schema.String) },
}).annotate({ description: 'One config read: a Vite build under production, a Vitest config under the test run environment and the flags its target passes, or a Vite config that is a suite where it states test' });
const Report = Schema.Struct({ folders: Schema.optionalKey(Schema.Array(Schema.String)) }).annotate({ description: 'Workspace-relative folders the run writes, absent where the config builds nothing or lists no test file' });
const Reported = Schema.Struct({ outputFile: Schema.optionalKey(Schema.String), outputDir: Schema.optionalKey(Schema.String) }).annotate({ description: 'The file and folder options of a resolved [reporter, options] Vitest entry' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const loaded = <A>(folder: string, ...specifiers: Array.NonEmptyReadonlyArray<string>) =>
    Effect.tryPromise({ try: (): Promise<A> => import(pathToFileURL(specifiers.reduce((from, specifier) => createRequire(from).resolve(specifier), `${folder}/package.json`)).href), catch: ToolError.of(specifiers.join(' > '), `does not load from ${folder}`) });

const outermost = (path: Path.Path, workspaceRoot: string, folders: readonly string[]) => Array.dedupe(folders.map((at) => path.relative(workspaceRoot, at))).filter((at, _, all) => !all.some((other) => at.startsWith(`${other}/`)));

const bundled = Effect.fnUntraced(function* ({ root, file, outDir }: typeof Job.cases.Bundle.Type, workspaceRoot: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const folder = path.join(workspaceRoot, root);
    const { resolveConfig } = yield* loaded<typeof import('vite')>(folder, 'vite');
    const config = yield* Effect.tryPromise({
        try: () => resolveConfig({ configFile: path.join(workspaceRoot, file), root: folder, logLevel: 'silent', build: outDir === undefined ? {} : { outDir } }, 'build', 'production', 'production'),
        catch: ToolError.of(file, 'does not resolve as a Vite config'),
    });
    const builds = [config.build, ...Object.values(config.environments).map((environment) => environment.build)];
    const buildable = config.build.lib !== false || config.builder?.buildApp !== undefined || builds.some((build) => build.rolldownOptions.input !== undefined) || (yield* Effect.mapError(fs.exists(path.join(config.root, 'index.html')), ToolError.of(file, 'has an unreadable root')));
    const folders = outermost(
        path,
        workspaceRoot,
        builds.map((build) => path.resolve(config.root, build.outDir)),
    );
    return { report: Report.make(buildable ? { folders } : {}), sources: config.configFileDependencies };
});

const collected = Effect.fnUntraced(function* ({ root: at, file, flags }: typeof Job.cases.Suite.Type | typeof Job.cases.Embedded.Type, workspaceRoot: string) {
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
    const probes = yield* Stream.fromIterable(projects).pipe(
        Stream.mapEffect(
            Effect.fnUntraced(function* ({ root: own, dir, include, exclude, includeSource = [], typecheck }) {
                const cwd = dir ?? own;
                const typed = typecheck.enabled && typecheck.only === true;
                const [tests, checks] = yield* Effect.all([matched(cwd, typed ? [] : include, exclude), matched(cwd, typecheck.enabled ? typecheck.include : [], typecheck.exclude)]);
                const globbed = tests.length + checks.length > 0;
                const sourced = yield* matched(cwd, typed || globbed ? [] : includeSource, exclude);
                const marked = yield* Effect.filter(sourced, (source) => Effect.map(Effect.option(fs.readFileString(source)), Option.exists(String.includes('import.meta.vitest'))));
                return { found: globbed || marked.length > 0, globbed, sourced };
            }),
        ),
        Stream.takeUntil(({ found }) => found),
        Stream.runCollect,
    );
    const listed = probes.some((probe) => probe.found);
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
    return { report: Report.make(listed ? { folders } : {}), sources: [config.configFileDependencies, ...resolvedProjects.map(({ viteConfig }) => viteConfig.configFileDependencies), ...(probes.some((probe) => probe.globbed) ? [] : probes.map((probe) => probe.sourced))].flat() };
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
    runtime.runPromise(served(Job, Report, (job, workspaceRoot) => Job.match(job, { Bundle: (build) => bundled(build, workspaceRoot), Suite: (suite) => collected(suite, workspaceRoot), Embedded: (suite) => embedded(suite, workspaceRoot) })));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Job, Report };
