import { isAbsolute, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import type { ProjectConfiguration, TargetConfiguration } from '@nx/devkit';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Effect, Formatter, Schema, type Scope, Stream } from 'effect';
import { ChildProcess, ChildProcessSpawner, CommandLine } from 'effect/process';
import type { Run } from '#/executors/run/executor';

// --- [TYPES] ---------------------------------------------------------------------------

type Tools = Readonly<Record<string, Readonly<Record<string, TargetConfiguration>>>>;

interface Part {
    readonly root: string;
    readonly name: string;
    readonly file: string;
    readonly technologies: readonly string[];
    readonly tools: Tools;
    readonly published?: TargetConfiguration;
    readonly release?: NonNullable<ProjectConfiguration['release']>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLUGIN = '@rasm/nx-apple';
const SHARED = distribution.name;
const GIT_STATUS = { GIT_CONFIG_COUNT: '1', GIT_CONFIG_KEY_0: 'status.branch', GIT_CONFIG_VALUE_0: 'false' } as const;
const SWIFTPM = ['--force-resolved-versions', '--no-color-diagnostics'] as const;
const INPUTS: NonNullable<TargetConfiguration['inputs']> = [{ runtime: 'node -p process.platform+process.arch' }, { env: 'DEVELOPER_DIR' }, { env: 'TOOLCHAINS' }, { externalDependencies: [] }];
const ATTACH: TargetConfiguration = {
    executor: `${SHARED}:attach`,
    dependsOn: ['pack--ditto'],
    inputs: [{ dependentTasksOutputFiles: '**/*' }],
    metadata: { description: "Uploads the files pack--ditto wrote to the GitHub release of the version the project's manifest states through gh release upload", help: { command: 'gh release upload --help', example: { options: { dryRun: true } } } },
};

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ trustPackages: Schema.optionalKey(Schema.Boolean), outputPath: Schema.optionalKey(Schema.String) }).annotate({
    description: 'trustPackages: passes -skipMacroValidation and -skipPackagePluginValidation to xcodebuild. outputPath: workspace folder holding archives and their packages under <outputPath>/xcode/<project>/<scheme> and product zips under <outputPath>/swiftpm/<product>, absent gives no pack--ditto target',
});
const Reason = Schema.TaggedUnion({ Exited: { code: Schema.Int, stderr: Schema.String }, Refused: { detail: Schema.String } });

// --- [ERRORS] --------------------------------------------------------------------------

class AppleError extends Schema.TaggedError<AppleError, { readonly brand: unique symbol }>()('AppleError', { reason: Reason, subject: Schema.String, cause: Schema.optionalKey(Schema.Defect()) }) {
    static readonly of = (reason: AppleError['reason'], subject: string) => (cause?: unknown) => AppleError.make({ reason, subject, ...(cause === undefined ? {} : { cause }) });

    override get message() {
        const said = Reason.match(this.reason, { Exited: ({ code, stderr }) => [`exits ${code}`, ...(stderr.trim() === '' ? [] : [stderr.trim()])].join(': '), Refused: ({ detail }) => detail });
        return [`${this.subject}: ${said}`, ...(this.cause === undefined ? [] : [Formatter.format(this.cause)])].join(': ');
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const refused = (subject: string, detail: string) => AppleError.of({ _tag: 'Refused', detail }, subject);

const analyzer = Effect.try({ try: () => fileURLToPath(import.meta.resolve('#analyzer')), catch: refused('#analyzer', 'does not resolve') });

const tokened = (at: string) => (isAbsolute(at) || at.startsWith('..') ? [] : [join('{workspaceRoot}', at)]);

const printed = Effect.fnUntraced(function* (argv: Array.NonEmptyReadonlyArray<string>, { input, ...options }: Pick<ChildProcess.CommandOptions, 'cwd' | 'env'> & { readonly input?: string }) {
    const handle = yield* ChildProcess.make(Array.headNonEmpty(argv), Array.tailNonEmpty(argv), { ...options, extendEnv: true, detached: false, stdin: input === undefined ? 'ignore' : Stream.encodeText(Stream.make(input)) });
    const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? stdout : yield* AppleError.of({ _tag: 'Exited', code, stderr }, CommandLine.display(argv))();
}, Effect.scoped);

const ran = (argv: Array.NonEmptyReadonlyArray<string>, options: Pick<ChildProcess.CommandOptions, 'cwd' | 'env'>) =>
    Effect.flatMap(ChildProcessSpawner.ChildProcessSpawner, (spawner) => spawner.exitCode(ChildProcess.make(Array.headNonEmpty(argv), Array.tailNonEmpty(argv), { ...options, extendEnv: true, detached: false, stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' })));

const target = (runs: (typeof Run.Type)['runs'], metadata: NonNullable<TargetConfiguration['metadata']>, { options, ...extra }: Partial<TargetConfiguration> = {}): TargetConfiguration => ({
    executor: `${PLUGIN}:run`,
    options: { runs, env: GIT_STATUS, ...options },
    metadata,
    ...extra,
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const run = <A, E>(effect: Effect.Effect<A, E, NodeServices | Scope.Scope>) => effect.pipe(Effect.tapCause(Effect.logError), Effect.scoped, Effect.provide(layer), Effect.runPromise);

// --- [EXPORTS] -------------------------------------------------------------------------

export { AppleError, ATTACH, analyzer, GIT_STATUS, INPUTS, Options, type Part, PLUGIN, printed, ran, refused, run, SHARED, SWIFTPM, type Tools, target, tokened };
