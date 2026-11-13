/// <reference types="vitest/config" />
import { matchesGlob } from 'node:path';
import { ancestors, MANIFESTS } from '@rasm/nx-devkit/plugin';
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
const Suite = Schema.Struct({ root: Schema.String, projects: Schema.Array(Schema.String), files: Schema.Array(Schema.String), folders: Schema.Array(Schema.String) }).annotate({
    description: 'The tests of one package: its workspace-relative folder, the Vitest project names that select them where the config holds other projects too, its test files relative to that folder in name order, and the workspace-relative folders its projects glob',
});
const Report = Schema.Struct({ run: Schema.optionalKey(Schema.Struct({ suites: Schema.Array(Suite), folders: Schema.Array(Schema.String), coverage: Schema.String, loaded: Schema.Array(Schema.String) })) }).annotate({
    description: 'The run of a config listing a test file: one suite per package holding the root of a project the config declares, workspace-relative folders the run writes, its workspace-relative coverage folder, and the workspace-relative files the config loaded',
});
const Reported = Schema.Struct({ outputFile: Schema.optionalKey(Schema.String), outputDir: Schema.optionalKey(Schema.String) }).annotate({ description: 'The file and folder options of a resolved [reporter, options] Vitest entry' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const collected = Effect.fnUntraced(function* ({ root: at, file, flags }: typeof Job.Type, workspaceRoot: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const [folder, own] = [path.join(workspaceRoot, at), path.join(workspaceRoot, file)];
    const [{ parseCLI, resolveConfig }, { glob }] = yield* Effect.all([loaded<typeof import('vitest/node')>(folder, 'vitest/node'), loaded<typeof import('tinyglobby')>(folder, 'vitest/package.json', 'tinyglobby')]);
    const config = yield* Effect.tryPromise({
        try: () => resolveConfig({ ...parseCLI(['vitest', 'run', ...flags]).options, config: own, root: folder, watch: false }, { logLevel: 'silent' }),
        catch: ToolError.of(file, 'does not resolve as a Vitest config'),
    });
    const { root, coverage, attachmentsDir, outputFile, reporters, resolvedProjects } = config.test;
    const declared = resolvedProjects.filter(({ inline, viteConfig }) => inline === true || viteConfig.configFile === own);
    const parents = new Map(declared.filter(({ hidden }) => hidden === true).map(({ viteConfig, projectConfig }) => [viteConfig, projectConfig.name] as const));
    const matched = (cwd: string, patterns: readonly string[], exclude: readonly string[]) =>
        Array.match(patterns, { onEmpty: () => Effect.succeed(Array.empty<string>()), onNonEmpty: (include) => Effect.tryPromise({ try: () => glob(include, { cwd, ignore: [...exclude], dot: true, expandDirectories: false, absolute: true }), catch: ToolError.of(file, `lists no test file under ${cwd}`) }) });
    const held = (inside: string) => Effect.map(fs.readDirectory(path.join(workspaceRoot, inside)), (names) => names.some((name) => matchesGlob(name, MANIFESTS)));
    const probes = yield* Effect.forEach(
        declared.filter(({ hidden }) => hidden !== true),
        Effect.fnUntraced(function* ({ viteConfig, projectConfig: { name, root: base, dir, include, exclude, includeSource = [], typecheck, browser } }) {
            const cwd = dir ?? base;
            const typed = typecheck.enabled && typecheck.only === true;
            const [tests, checks, sourced] = yield* Effect.all([matched(cwd, typed ? [] : include, exclude), matched(cwd, typecheck.enabled ? typecheck.include : [], typecheck.exclude), matched(cwd, typed ? [] : includeSource, exclude)]);
            const marked = yield* Effect.filter(sourced, (source) => Effect.map(Effect.option(fs.readFileString(source)), Option.exists(String.includes('import.meta.vitest'))));
            const owner = yield* Effect.mapError(Effect.flatMap(Effect.findFirst(ancestors(path, path.relative(workspaceRoot, base) || '.'), held), Effect.fromOption), ToolError.of(file, `declares a project under ${base}, which no package holds`));
            return { owner, project: parents.get(viteConfig) ?? name, files: [...tests, ...checks, ...marked], sourced, cwd, written: browser.enabled ? Array.getSomes([Option.fromUndefinedOr(browser.trace.tracesDir), Option.fromNullishOr(browser.screenshotDirectory)]) : [] };
        }),
    );
    const selectable = Array.dedupe(probes.map(({ project }) => project));
    const suites = Record.collect(
        Array.groupBy(probes, ({ owner }) => owner),
        (owner, group) => {
            const selected = Array.dedupe(group.map(({ project }) => project));
            return {
                root: owner,
                projects: selected.length === selectable.length ? [] : selected,
                files: Array.dedupe(group.flatMap((probe) => probe.files).map((spec) => path.relative(path.join(workspaceRoot, owner), spec))).toSorted(),
                folders: Array.dedupe(group.map(({ cwd }) => path.relative(workspaceRoot, cwd) || '.')),
            };
        },
    ).filter(({ files }) => files.length > 0);
    const stated = Option.liftPredicate(Predicate.isString);
    const keyed: Readonly<Record<string, string | undefined>> = Predicate.isString(outputFile) ? {} : (outputFile ?? {});
    const reports = reporters.flatMap((reporter) => {
        const [name, options] = Array.isArray(reporter) ? reporter : [reporter, {}];
        const decoded = Schema.decodeUnknownOption(Reported)(options);
        const named = Option.flatMap(stated(name), (key) => Option.fromUndefinedOr(keyed[key]));
        const given = Array.getSomes([Option.firstSomeOf([Option.flatMap(decoded, ({ outputFile: direct }) => Option.fromUndefinedOr(direct)), stated(outputFile), named]), Option.flatMap(decoded, ({ outputDir }) => Option.fromUndefinedOr(outputDir))]);
        return Array.isReadonlyArrayNonEmpty(given) ? given : Option.toArray(Option.flatMap(stated(name), (key) => Record.get(REPORTS, key)));
    });
    const folders = outermost(
        path,
        workspaceRoot,
        [coverage.reportsDirectory, attachmentsDir, ...reports, ...probes.flatMap((probe) => probe.written)].map((written) => path.resolve(root, written)),
    );
    const sources = [own, config.configFileDependencies, ...resolvedProjects.map(({ viteConfig }) => viteConfig.configFileDependencies), ...probes.map((probe) => probe.sourced)].flat();
    const depended = Array.dedupe([own, ...config.configFileDependencies, ...declared.flatMap(({ viteConfig }) => viteConfig.configFileDependencies)].map((dependency) => path.relative(workspaceRoot, dependency)));
    const run = { suites, folders, coverage: path.relative(workspaceRoot, path.resolve(root, coverage.reportsDirectory)), loaded: depended };
    return { report: Report.make(Array.isReadonlyArrayNonEmpty(suites) ? { run } : {}), sources };
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
