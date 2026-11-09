import { createRequire, registerHooks } from 'node:module';
import process from 'node:process';
import { pathToFileURL } from 'node:url';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Array, type Cause, Effect, FileSystem, Option, Path, Predicate, Record, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Configured = keyof typeof READS;
interface Cli {
    readonly parse: (argv: readonly string[], options: { readonly run: boolean }) => unknown;
    readonly runMatchedCommand: () => Promise<unknown>;
}
interface Vitest {
    readonly config: unknown;
    readonly projects: readonly { readonly config: unknown }[];
    readonly close: () => Promise<void>;
}

// --- [MODELS] --------------------------------------------------------------------------

const Built = Schema.Struct({ outDir: Schema.String });
const Vite = Schema.Struct({ build: Built, environments: Schema.Record(Schema.String, Schema.Struct({ build: Built })) }).annotate({ description: 'The resolved Vite config fields naming every folder vite build writes' });
const Traced = Schema.Struct({ browser: Schema.Struct({ enabled: Schema.Boolean, trace: Schema.Struct({ tracesDir: Schema.optionalKey(Schema.NullOr(Schema.String)) }) }) }).annotate({
    description: 'The resolved Vitest project field naming the folder its browser run writes traces into',
});
const Run = Schema.Struct({
    root: Schema.String,
    coverage: Schema.Struct({ reportsDirectory: Schema.String }),
    attachmentsDir: Schema.String,
    outputFile: Schema.optionalKey(Schema.Union([Schema.String, Schema.Record(Schema.String, Schema.String)])),
    reporters: Schema.Array(Schema.Unknown),
}).annotate({ description: 'The resolved root Vitest config fields one run shares across its projects: coverage reports, attachments, and each reporter file' });
const Report = Schema.fromJsonString(Schema.Struct({ present: Schema.Boolean, written: Schema.Array(Schema.String), loaded: Schema.Array(Schema.String) })).annotate({
    description: 'The report the plugin reads: whether the runner finds a test, every path the resolved config writes, and every module the read resolved',
});
const Files = Schema.fromJsonString(Schema.Array(Schema.Struct({ file: Schema.String }))).annotate({ description: 'vitest list --filesOnly --json: every test file each Vitest project finds' });
const Reporter = Schema.Tuple([Schema.Unknown, Schema.Struct({ outputFile: Schema.optionalKey(Schema.Unknown), outputDir: Schema.optionalKey(Schema.Unknown) })]).annotate({
    description: 'A resolved [reporter, options] entry, a reporter instance decoding to none',
});

// --- [TABLES] --------------------------------------------------------------------------

const READS = {
    vite: Effect.fnUntraced(function* (load: <A>(specifier: string) => Effect.Effect<A, Cause.UnknownError>, file: string, root: string) {
        const { resolveConfig } = yield* load<{ readonly resolveConfig: (config: object, command: string, mode: string, environment: string) => Promise<unknown> }>('vite');
        const { build, environments } = yield* Effect.tryPromise(() => resolveConfig({ configFile: file, root, logLevel: 'silent' }, 'build', 'production', 'production')).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Vite)));
        return { present: true, root, written: [build.outDir, ...Object.values(environments).map((environment) => environment.build.outDir)] };
    }),
    vitest: Effect.fnUntraced(function* (load: <A>(specifier: string) => Effect.Effect<A, Cause.UnknownError>, file: string, root: string) {
        const [fs, path, { createCLI, createVitest }] = yield* Effect.all([FileSystem.FileSystem, Path.Path, load<{ readonly createCLI: () => Cli; readonly createVitest: (mode: string, options: object, overrides: object) => Promise<Vitest> }>('vitest/node')]);
        const listing = path.join(yield* fs.makeTempDirectoryScoped(), 'listing.json');
        const cli = createCLI();
        cli.parse(['', '', 'list', '--filesOnly', `--json=${listing}`, `--config=${file}`, `--root=${root}`], { run: false });
        yield* Effect.tryPromise(() => cli.runMatchedCommand());
        const vitest = yield* Effect.acquireRelease(
            Effect.tryPromise(() => createVitest('test', { config: file, root, watch: false, run: true }, { logLevel: 'silent' })),
            (opened) => Effect.promise(() => opened.close()),
        );
        const [run, projects, files] = yield* Effect.all([Schema.decodeUnknownEffect(Run)(vitest.config), Effect.forEach(vitest.projects, ({ config }) => Schema.decodeUnknownEffect(Traced)(config)), Effect.flatMap(fs.readFileString(listing), Schema.decodeEffect(Files))]);
        const { outputFile: stated = {} } = run;
        const shared = (name: unknown) => (Predicate.isString(stated) ? Option.some(stated) : Option.flatMap(Option.liftPredicate(name, Predicate.isString), (key) => Record.get(stated, key)));
        return {
            present: files.length > 0,
            root: run.root,
            written: [
                run.coverage.reportsDirectory,
                run.attachmentsDir,
                ...Array.getSomes(run.reporters.map((reporter) => Schema.decodeUnknownOption(Reporter)(reporter))).flatMap(([name, { outputFile, outputDir }]) =>
                    Array.getSomes([Option.orElse(Option.liftPredicate(outputFile, Predicate.isString), () => shared(name)), Option.liftPredicate(outputDir, Predicate.isString)]),
                ),
                ...Array.getSomes(projects.map(({ browser }) => (browser.enabled ? Option.fromNullishOr(browser.trace.tracesDir) : Option.none()))),
            ],
        };
    }, Effect.scoped),
} as const;
const Arguments = Schema.Tuple([Schema.Literals(Record.keys(READS)), Schema.String, Schema.String]).annotate({ description: 'argv: the read, the config file, the report file' });

// --- [COMPOSITION] ---------------------------------------------------------------------

if (import.meta.main) {
    runMain(
        Effect.gen(function* () {
            const [fs, path, [kind, file, report]] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Schema.decodeUnknownEffect(Arguments)(process.argv.slice(2))]);
            const loaded = new Set<string>();
            registerHooks({
                resolve: (specifier, context, next) => {
                    const resolution = next(specifier, context);
                    loaded.add(resolution.url);
                    return resolution;
                },
            });
            const root = process.cwd();
            const load = <A>(specifier: string) => Effect.tryPromise((): Promise<A> => import(pathToFileURL(createRequire(path.join(root, 'package.json')).resolve(specifier)).href));
            const { present, root: base, written } = yield* READS[kind](load, file, root);
            yield* fs.writeFileString(report, yield* Schema.encodeEffect(Report)({ present, written: written.map((at) => path.resolve(base, at)), loaded: [...loaded] }));
        }).pipe(Effect.provide(layer)),
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Configured, Report };
